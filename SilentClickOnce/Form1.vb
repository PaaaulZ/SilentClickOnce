Imports System.Deployment.Application
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Public Class Form1
    Dim WithEvents iphm As InPlaceHostingManager = Nothing

    Public Sub InstallApplication(ByVal deployManifestUriStr As String)

        Try
            ' Try installing the application
            Dim deploymentUri As New Uri(deployManifestUriStr)
            iphm = New InPlaceHostingManager(deploymentUri, False)
            Console.WriteLine("[?] Starting setup.")
        Catch uriEx As UriFormatException
            Console.WriteLine("[-] Unable to install, invalid URL. Error: " + uriEx.Message)
            Environment.Exit(0)
        Catch platformEx As PlatformNotSupportedException
            Console.WriteLine("[-] Unable to install, unsupported platform. Error: " + platformEx.Message)
            Environment.Exit(0)
        Catch argumentEx As ArgumentException
            Console.WriteLine("[-] Unable to install, invalid argument. Error: " + argumentEx.Message)
            Environment.Exit(0)
        End Try

        iphm.GetManifestAsync()

    End Sub


    Private Sub iphm_GetManifestCompleted(ByVal sender As Object, ByVal e As GetManifestCompletedEventArgs) Handles iphm.GetManifestCompleted
        ' Check for errors downloading the manifest.
        If (e.Error IsNot Nothing) Then
            Console.WriteLine("[-] Error verifying manifest. Error: " + e.Error.Message)
            Environment.Exit(0)
        End If

        ' Check for requirements
        Try
            iphm.AssertApplicationRequirements(True)
        Catch ex As Exception
            Console.WriteLine("[-] Error verifying requirements. Error: " + ex.Message)
            Environment.Exit(0)
        End Try

        ' Download application
        Try
            iphm.DownloadApplicationAsync()
        Catch downloadEx As Exception
            Console.WriteLine("[-] Error downloading. Error: " + downloadEx.Message)
            Environment.Exit(0)
        End Try
    End Sub

    Private Sub iphm_DownloadApplicationCompleted(ByVal sender As Object, ByVal e As DownloadApplicationCompletedEventArgs) Handles iphm.DownloadApplicationCompleted

        ' Check for errors downloading the application
        If (e.Error IsNot Nothing) Then
            Console.WriteLine("[-] Error installing: " & e.Error.Message)
            Environment.Exit(0)
        End If

        ' Application installed
        Console.WriteLine("[+] Installation completed.")
        Environment.Exit(0)
    End Sub


    Private Sub InstallUpdateSyncWithInfo()

        Dim info As UpdateCheckInfo = Nothing

        If ApplicationDeployment.IsNetworkDeployed Then

            Dim ad As ApplicationDeployment = ApplicationDeployment.CurrentDeployment
            Try
                info = ad.CheckForDetailedUpdate()
            Catch dde As DeploymentDownloadException
                Console.WriteLine("[-] Cannot download application. " + dde.Message)
                Return
            Catch ide As InvalidDeploymentException
                Console.WriteLine("[-] Cannot check for new version, corrupted ClickOnce? " + ide.Message)
                Return
            Catch ioe As InvalidOperationException
                Console.WriteLine("[-] Cannot update, not a ClickOnce?. " + ioe.Message)
                Return
            End Try

            If info.UpdateAvailable Then

                Try
                    ad.Update()
                    MessageBox.Show("[+] Application updated, restarting")
                    Application.Restart()
                Catch dde As DeploymentDownloadException
                    Console.WriteLine("[-] Cannot update: " + dde.Message)
                    Return
                End Try
            End If
        End If

    End Sub

    Private Sub Uninstall(ByVal applicationName As String)

        ' Kill process if open
        Try
            For Each p As Process In Process.GetProcessesByName(applicationName)
                p.Kill()
                Exit For
            Next

            Dim uninstallString As String = GetUninstallCommand(applicationName)
            Dim fullApplicationName As String = ""

            If uninstallString.Contains($"{applicationName}.application") Then
                fullApplicationName = $"{applicationName}.application"
            Else
                fullApplicationName = $"{applicationName}.app"
            End If

            Dim publicKeyToken As String = uninstallString.Replace($"rundll32.exe dfshim.dll,ShArpMaintain {fullApplicationName}, Culture=neutral, PublicKeyToken=", "")
            publicKeyToken = publicKeyToken.Substring(0, publicKeyToken.IndexOf(","))
            Dim processorArchitecture As String = uninstallString.Replace($"rundll32.exe dfshim.dll,ShArpMaintain {fullApplicationName}, Culture=neutral, PublicKeyToken={publicKeyToken}, processorArchitecture=", "")

            Dim textualSubId As String = $"{fullApplicationName}, Culture=neutral, PublicKeyToken={publicKeyToken}, processorArchitecture={processorArchitecture}"
            Dim deploymentServiceCom As New System.Deployment.Application.DeploymentServiceCom()
            Dim _r_m_GetSubscriptionState As Reflection.MethodInfo = GetType(System.Deployment.Application.DeploymentServiceCom).GetMethod("GetSubscriptionState", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)
            Dim subState As Object = _r_m_GetSubscriptionState.Invoke(deploymentServiceCom, New Object() {textualSubId})
            Dim subscriptionStore As Object = subState.GetType().GetProperty("SubscriptionStore").GetValue(subState)
            subscriptionStore.GetType().GetMethod("UninstallSubscription").Invoke(subscriptionStore, New Object() {subState})

            Console.WriteLine("[+] Succesfully uninstalled")

            Environment.Exit(0)

        Catch ex As Exception
            Console.WriteLine($"[-] Error uninstalling {ex.Message}")
            Environment.Exit(1)
        End Try
    End Sub

    Private Function GetUninstallCommand(ByVal applicationName As String)

        ' Search for the uninstall string in the Windows registry

        Dim key As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Uninstall")

        If key Is Nothing Then
            Return "NO_SUBKEYS"
        End If

        For Each subKey In key.GetSubKeyNames()

            Dim appTMP As RegistryKey = key.OpenSubKey(subKey)

            If appTMP Is Nothing Then
                Continue For
            End If

            For Each appKeyTMP In appTMP.GetValueNames().Where(Function(x) x.Equals("DisplayName"))
                If appTMP.GetValue(appKeyTMP).Equals(applicationName) Then
                    Return appTMP.GetValue("UninstallString")
                End If
            Next

        Next

        Return Nothing

    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Visible = False

        Dim arguments() As String = Environment.GetCommandLineArgs()
        If (arguments.Count <= 1) Then
            Console.WriteLine("[-] Missing argument (-i .application url OR -u application name)")
            Environment.Exit(0)
        Else
            If arguments(1).Equals("-i") Then
                Dim installer As New Form1
                installer.InstallApplication(arguments(2))
            ElseIf arguments(1).Equals("-u") Then
                Uninstall(arguments(2))
            Else
                Console.WriteLine("[-] Unknown arguments passed")
            End If
        End If
    End Sub

End Class
