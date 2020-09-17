Imports System.Deployment.Application

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

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' On start hide the form and launch the setup

        Me.Visible = False

        Dim arguments() As String = Environment.GetCommandLineArgs()
        If (arguments.Count <= 1) Then
            Console.WriteLine("[-] Missing argument (.application URL)")
            Environment.Exit(0)
        Else
            Dim installer As New Form1
            installer.InstallApplication(arguments(1))
        End If
    End Sub

End Class
