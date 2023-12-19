Imports System.Deployment.Application
Imports Microsoft.Win32
Imports System.Text.RegularExpressions
Imports System.Reflection

Public Class Form1
    Private WithEvents iphm As InPlaceHostingManager = Nothing

    Public Sub InstallApplication(ByVal deployManifestUriStr As String)

        Try
            ' Try installing the application
            Dim deploymentUri As New Uri(deployManifestUriStr)
            iphm = New InPlaceHostingManager(deploymentUri, False)
            WriteLog("[?] Starting setup.")
        Catch uriEx As UriFormatException
            WriteLog($"[-] Unable to install, invalid URL: {uriEx.Message}")
            Environment.Exit(0)
        Catch platformEx As PlatformNotSupportedException
            WriteLog($"[-] Unable to install, unsupported platform: {platformEx.Message}")
            Environment.Exit(0)
        Catch argumentEx As ArgumentException
            WriteLog($"[-] Unable to install, invalid argument: {argumentEx.Message}")
            Environment.Exit(0)
        End Try

        iphm.GetManifestAsync()

    End Sub


    Private Sub iphm_GetManifestCompleted(ByVal sender As Object, ByVal e As GetManifestCompletedEventArgs) Handles iphm.GetManifestCompleted
        ' Check for errors downloading the manifest.
        If (e.Error IsNot Nothing) Then
            WriteLog($"[-] Error verifying manifest: {e.Error.Message}")
            Environment.Exit(0)
        End If

        ' Check for requirements
        Try
            iphm.AssertApplicationRequirements(True)
        Catch ex As Exception
            WriteLog($"[-] Error verifying requirements: {ex.Message}")
            Environment.Exit(0)
        End Try

        ' Download application
        Try
            iphm.DownloadApplicationAsync()
        Catch downloadEx As Exception
            WriteLog($"[-] Error downloading: {downloadEx.Message}")
            Environment.Exit(0)
        End Try
    End Sub

    Private Sub iphm_DownloadApplicationCompleted(ByVal sender As Object, ByVal e As DownloadApplicationCompletedEventArgs) Handles iphm.DownloadApplicationCompleted

        ' Check for errors downloading the application
        If (e.Error IsNot Nothing) Then
            WriteLog($"[-] Error installing: {e.Error.Message}")
            Environment.Exit(0)
        End If

        ' Application installed
        WriteLog("[+] Installation completed.")
        Environment.Exit(0)
    End Sub


    Private Sub InstallUpdateSyncWithInfo()

        Dim info As UpdateCheckInfo = Nothing

        If ApplicationDeployment.IsNetworkDeployed Then

            Dim ad As ApplicationDeployment = ApplicationDeployment.CurrentDeployment
            Try
                info = ad.CheckForDetailedUpdate()
            Catch dde As DeploymentDownloadException
                WriteLog($"[-] Cannot download application: {dde.Message}")
                Return
            Catch ide As InvalidDeploymentException
                WriteLog($"[-] Cannot check for new version, corrupted ClickOnce? {ide.Message}")
                Return
            Catch ioe As InvalidOperationException
                WriteLog($"[-] Cannot update, not a ClickOnce? {ioe.Message}")
                Return
            End Try

            If info.UpdateAvailable Then

                Try
                    ad.Update()
                    MessageBox.Show("[+] Application updated, restarting")
                    Application.Restart()
                Catch dde As DeploymentDownloadException
                    WriteLog($"[-] Cannot update: {dde.Message}")
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

            Dim fullApplicationName As String = ""
            Dim uninstallString As String = GetUninstallCommand(applicationName)

            If IsNothing(uninstallString) Then
                WriteLog($"[-] Application '{applicationName}' not found in registry, trying all users")
                Dim userSIDs As ArrayList = getAvailableUsersSID()
                For Each userSID In userSIDs
                    uninstallString = GetUninstallCommand(applicationName, userSID)
                    If Not IsNothing(uninstallString) Then
                        Exit For
                    End If
                Next

                If IsNothing(uninstallString) Then
                    WriteLog($"[-] Application '{applicationName}' still not found in registry")
                    Environment.Exit(1)
                End If
            End If

            If uninstallString.Contains($"{applicationName}.application") Then
                fullApplicationName = $"{applicationName}.application"
            Else
                fullApplicationName = $"{applicationName}.app"
            End If

            Dim extractedInfo As String() = ExtractInfo(uninstallString)

            WriteLog($"[?] PublicKeyToken: {extractedInfo(0)}, Culture: {extractedInfo(1)}, processorArchitecture: {extractedInfo(2)}")
            WriteLog($"[?] Uninstall string: {uninstallString}")
            If IsNothing(extractedInfo(0)) Or IsNothing(extractedInfo(1)) Or IsNothing(extractedInfo(2)) Then
                WriteLog("[-] Some data is missing from uninstall string ")
                Environment.Exit(1)
            End If

            Dim textualSubId As String
            If uninstallString.Contains("rundll32.exe dfshim.dll,ShArpMaintain ") Then
                textualSubId = uninstallString.Replace("rundll32.exe dfshim.dll,ShArpMaintain ", "")
                WriteLog("[?] Using new method")
            Else
                textualSubId = $"{fullApplicationName}, Culture={extractedInfo(1)}, PublicKeyToken={extractedInfo(0)}, processorArchitecture={extractedInfo(2)}"
                WriteLog("[?] Using old method")
            End If

            Dim deploymentServiceCom As New System.Deployment.Application.DeploymentServiceCom()
            Dim _r_m_GetSubscriptionState As Reflection.MethodInfo = GetType(System.Deployment.Application.DeploymentServiceCom).GetMethod("GetSubscriptionState", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)
            Dim subState As Object = _r_m_GetSubscriptionState.Invoke(deploymentServiceCom, New Object() {textualSubId})
            Dim subscriptionStore As Object = subState.GetType().GetProperty("SubscriptionStore").GetValue(subState)
            subscriptionStore.GetType().GetMethod("UninstallSubscription").Invoke(subscriptionStore, New Object() {subState})

            WriteLog("[+] Succesfully uninstalled")

            Environment.Exit(0)

        Catch ex As Exception
            WriteLog($"[-] Error uninstalling: {ex.InnerException}{Environment.NewLine}{Environment.NewLine}---------{ex.Message}{Environment.NewLine}{Environment.NewLine}---------{ex.StackTrace}")
            Environment.Exit(1)
        End Try
    End Sub

    Private Function getAvailableUsersSID()

        Dim sids As New ArrayList

        Dim keys() As String = Registry.Users.GetSubKeyNames()
        For Each key In keys
            If key.Length > 8 And Not key.Contains("_Classes") Then
                sids.Add(key)
            End If
        Next

        Return sids

    End Function

    Private Function GetUninstallCommand(ByVal applicationName As String, Optional ByVal userSID As String = "")

        ' Search for the uninstall string in the Windows registry

        Dim key As RegistryKey = Nothing

        If userSID = "" Then
            key = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Uninstall")
        Else
            key = Registry.Users.OpenSubKey($"{userSID}\Software\Microsoft\Windows\CurrentVersion\Uninstall")
        End If

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
            WriteLog("[-] Missing argument (-i .application url OR -u application name)")
            Environment.Exit(0)
        Else
            If arguments(1).Equals("-i") Then
                Dim installer As New Form1
                installer.InstallApplication(arguments(2))
            ElseIf arguments(1).Equals("-u") Then
                Uninstall(arguments(2))
            Else
                WriteLog("[-] Unknown arguments passed")
            End If
        End If
    End Sub

    Private Sub WriteLog(ByVal logText As String)
        Console.WriteLine($"[{Date.Now.Year.ToString("D2")}-{Date.Now.Month.ToString("D2")}-{Date.Now.Day.ToString("D2")} {Date.Now.Hour.ToString("D2")}:{Date.Now.Minute.ToString("D2")}:{Date.Now.Second.ToString("D2")}] {logText}")
    End Sub

    Private Function ExtractInfo(ByVal uninstallString As String)

        Dim info() As String = {Nothing, Nothing, Nothing}

        Dim groups As GroupCollection = Regex.Match(uninstallString, "PublicKeyToken=(\w+)", RegexOptions.IgnoreCase).Groups
        If groups.Count > 0 Then
            info(0) = groups(1).Value
        End If

        groups = Regex.Match(uninstallString, "Culture=(\w+)", RegexOptions.IgnoreCase).Groups
        If groups.Count > 0 Then
            info(1) = groups(1).Value
        End If

        groups = Regex.Match(uninstallString, "ProcessorArchitecture=(\w+)", RegexOptions.IgnoreCase).Groups
        If groups.Count > 0 Then
            info(2) = groups(1).Value
        End If

        Return info

    End Function

End Class