Option Explicit On

Imports System.Xml
Imports System.Net
Imports System.Net.Sockets
Imports System.IO.Ports
Imports System.Text
Imports System.Threading
Imports System.ComponentModel
Public Class frmConsole

#Region "Declaration"
    Dim HandlerNumber As Integer = 0
    Dim CheckCommand As String = ""

    Public ComPort As IO.Ports.SerialPort
    Dim Q As Queue(Of String) = New Queue(Of String)

    Public Structure sttConfig

        Public ComPortName As String
        Public WeightName As String
        Public FlagWeight As Boolean
        Public MaxClient As Integer
        Public ListenPort As Integer
        Public Visible As Boolean
        Public BaudRate As Integer
        Public DataBits As Integer

    End Structure

    Private Enum enuProcessType
        Indentify = 0
        Verify = 1
        Enroll = 1
    End Enum

    Private Config As sttConfig

    Private MAXCLIENTS As Integer = 2
    Private CONFIGURATIONFILE As String = Application.StartupPath & "/Config/Config.xml"
    Private Const CONFIG_SelectNodes As String = "Configurations/Configuration"
    Private Const CONFIG_ComPortName As String = "ComPortName"
    Private Const CONFIG_WeightName As String = "WeightName"
    Private Const CONFIG_MaxClient As String = "MaxClient"
    Private Const CONFIG_ListenPort As String = "ListenPort"
    Private Const CONFIG_Visible As String = "Visible"
    Private Const CONFIG_BaudRate As String = "BaudRate"
    Private Const CONFIG_DataBits As String = "DataBits"


    ' TCP COMMANDS
    Private Const COMMAND_INDENTITY = "IDT"
    Private Const COMMAND_VERIFY = "VRY"
    Private Const COMMAND_ENROLL = "ENR"
    Private Const RETURN_INIT = "INT"
    Private Const RETURN_START = "STT"
    Private Const RETURN_STOP = "STP"
    Private Const RETURN_DATA = "DAT"
    Private Const RETURN_ERROR = "ERR"
    Private Const COMMAND_WEIGHT = "WEI"
    Private Const RETURN_SPLITER = "##"
    Private Const COMMAND_RETURN_STOP = "CMD-STP##"

    Private Const VPROD_CLIENT_NAME = "VPROD"

    Private listenerIP As IPAddress
    Private listenerPort As Integer
    Private ExitApp As Boolean = False

    Private bwListener As BackgroundWorker
    Private listenerSocket As Socket
    Private processSocket As Socket
    Private connectCount As Integer = 0


    Private _Reading As Boolean = False
    Private _Reading_Started As Boolean = False
    Private sb As New StringBuilder()
    Private ProcessType As enuProcessType
    Private rBuffer(255) As Byte


#End Region


#Region "Constructor"
    Public Sub New()

        'This call Is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Initialize_Configuration()

    End Sub

    Protected Overrides Sub Finalize()
        Try

            lbWeight.Text = ""
            bwListener = Nothing

            MyBase.Finalize()
        Catch ex As Exception

        End Try

    End Sub

#End Region


#Region "Events"
    Private Sub frmConsole_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.MAXCLIENTS = Config.MaxClient
        Me.listenerIP = IPAddress.Any
        Me.listenerPort = Config.ListenPort

        Me.StartListening()

        If Not Config.Visible Then
            Me.WindowState = FormWindowState.Minimized
            Me.ShowInTaskbar = False
        End If

    End Sub

    Private Sub frmConsole_ClientSizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.ClientSizeChanged
        Try
            Dim form As Form = CType(sender, Form)
            If form.WindowState = FormWindowState.Maximized Or (form.WindowState = FormWindowState.Minimized And Me.Config.Visible = True) Then
                form.WindowState = FormWindowState.Normal

                Me.Left = (Screen.PrimaryScreen.WorkingArea.Width - Me.Width)
                Me.Top = (Screen.PrimaryScreen.WorkingArea.Height - Me.Height)
            End If
        Catch ex As Exception
            ' Write_Log("frmConsole_ClientSizeChanged : " & ex.Message)
        End Try
    End Sub

    Private Sub frmConsole_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        Try

            Me.Left = (Screen.PrimaryScreen.WorkingArea.Width - Me.Width)
            Me.Top = (Screen.PrimaryScreen.WorkingArea.Height - Me.Height)
        Catch ex As Exception

        End Try
    End Sub

#End Region


#Region "Methods"

    Private Sub Initialize_Connection()
        Try
            Me.listenerSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP)
            Me.listenerSocket.Bind(New IPEndPoint(Me.listenerIP, Me.listenerPort))
            Me.listenerSocket.Listen(200)
            While True
                AcceptMethod(listenerSocket.Accept)
            End While
            'Me.listenerSocket.BeginAccept(New AsyncCallback(AddressOf Me.OnConnectRequest), Me.listenerSocket)
        Catch ex As Exception

        End Try


    End Sub

    Protected Sub AcceptMethod(ByVal listeningSocket As Socket)
        Try
            AccecptRequest(listeningSocket)
        Catch ex As Exception

        End Try

    End Sub

    Public Function AccecptRequest(ByVal incomingSocket As Socket) As Boolean
        Dim recieveData As AsyncCallback = New AsyncCallback(AddressOf Me.OnRecievedData)

        Try
            'If Main.Reader.Connected = False Then Main.Reader.ConnectReader()
            Me._Reading = True
            'RaiseEvent ChangeStatus("Handler " & Me._ClientNumber & " : accept connection from " & incomingSocket.RemoteEndPoint.ToString)
            Me.sb = New StringBuilder
            Me.processSocket = incomingSocket
            SendData(RETURN_INIT)
            ''RaiseEvent ChangeStatus("Accept connection " & socket.RemoteEndPoint.ToString)
            Me.processSocket.BeginReceive(rBuffer, 0, rBuffer.Length, SocketFlags.None, recieveData, Me)
            'Else
            Return True
            'End If
        Catch ex As Exception

            Me._Reading = False
            Return False
            'Debug.Print("Handler " & Me.ClientNumber & " AccecptRequest : " & ex.Message)
        End Try
    End Function

    Private Sub SendData(ByVal dataString As String)
        ' Dim deMessage As New MethodDelegateScanState()
        Dim byteDateLine As Byte()
        CheckCommand = dataString ' keep for state on OnreceiveData 'ND14194

        Try
            ''RaiseEvent ChangeStatus("SendCommand : " & dataString)

            dataString = "CMD-" & dataString & RETURN_SPLITER & Environment.NewLine
            byteDateLine = System.Text.Encoding.ASCII.GetBytes(dataString.ToCharArray())

            If Not Me.processSocket Is Nothing AndAlso processSocket.Connected Then
                Me.processSocket.Send(byteDateLine)
                '    Me.Invoke(deMessage, "Command - Sent - " & dataString)
            End If

            ' deMessage = Nothing

        Catch ex As Exception
            ' NN 30052019
            ' Me.processSocket.Close()
            'MessageError("SendData")
        End Try
    End Sub


    Private Sub BeginProcess()

        Try
            'Private Sub BeginProcess(ByVal cmdString As String)
            Dim cmdString As String = Me.sb.ToString
            'Create new thread to read RFID Tags

            If ProcessCommand(cmdString) = False Then
                'RaiseEvent ChangeStatus(Me.socket.RemoteEndPoint.ToString & " : Error")
                SendData(RETURN_ERROR & " : " & cmdString)
                SendData(RETURN_STOP)
                Me._Reading_Started = False
                Me._Reading = False
                Me.processSocket.Close()
                'Me.Finalize()
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Function ProcessCommand(ByVal cmdString As String) As Boolean
        'Dim deFunctionStart As New MethodDelegateScanStart(AddressOf Me.Start_Identification)
        'Dim deFunctionStop As New MethodDelegateScanStart(AddressOf Me.Stop_Scanning)
        'Dim deFunctionMessage As New MethodDelegateScanState(AddressOf Me.UpdateConsole)
        Dim tmpString As String
        Dim sptString() As String
        Dim datastring As String
        Try
            Config.FlagWeight = False

            'cmdString = "CMD-WEI##"

            'Me.Invoke(deFunctionMessage, "Command - Received - " & cmdString)
            'RaiseEvent ChangeStatus("Processing command")
            tmpString = cmdString.Replace(RETURN_SPLITER, "")
            sptString = tmpString.Split("-")
            If sptString.Length > 1 Then
                SendData(RETURN_START)
                Me.sb = New StringBuilder
                If sptString(1) = COMMAND_WEIGHT Then
                    'INDENTIFY EMPLOYEE
                    Me.ProcessType = enuProcessType.Indentify
                    'Me.Result.Employee = ""
                    'Me.Result.FARNLevel = 0

                    ' NN to start reading
                    ' send result back

                    ' GetWeight()

                    Dim PortName As String = Config.ComPortName

                    ClearSerielPortBuffer()

                    MSCOM = New IO.Ports.SerialPort(PortName)

                    With MSCOM
                        .BaudRate = Config.BaudRate
                        .DataBits = Config.DataBits
                        .Parity = System.IO.Ports.Parity.Odd
                        .StopBits = StopBits.One
                        .Handshake = Handshake.XOnXOff
                        .DtrEnable = True
                        .RtsEnable = True
                        .ReceivedBytesThreshold = 16
                        .ReadTimeout = 1000
                    End With

                    MSCOM.Open()
                    If MSCOM.IsOpen Then
                        MSCOM.Write(Chr(27) + "P")
                    End If

                    Do Until Config.FlagWeight = True
                        Application.DoEvents()
                    Loop

                    datastring = "IDT-WEI-" & lbWeight.Text & RETURN_SPLITER & Environment.NewLine
                    SendData(datastring)

                    'SendData(RETURN_STOP)
                    ' to write message on screen

                    'Console.WriteLine("Weight : " & txtWeight.Text)

                    ' StartFingerPrintScanner()
                ElseIf sptString(1) = COMMAND_VERIFY Then
                    'VERIFY EMPLOYEE
                    Me.ProcessType = enuProcessType.Verify
                    ' Me.Result.Employee = sptString(2)
                    'Me.Result.FARNLevel = 0

                    ' NN NOT use
                    'Me.Invoke(deFunctionStart)


                    'StartFingerPrintScanner()
                Else
                    'ANY OTHER COMMAND JUST STOP SCANNING
                    ' StopFingerPrintScanner()
                    'Me.Invoke(deFunctionStop)
                    ' NN NOT use

                End If

                Application.DoEvents()

                'RaiseEvent ChangeStatus(Me.socket.RemoteEndPoint.ToString & " : Timeout : " & Main.Reader.ScanTimeout)
                'RaiseEvent ChangeStatus(Me.socket.RemoteEndPoint.ToString & " : Command OK")
                Return True
            Else
                'RaiseEvent ChangeStatus(Me.socket.RemoteEndPoint.ToString & " : Command not ok")
                Return False
            End If
        Catch ex As Exception
            Application.DoEvents()
            SendData("NFW")
            Return True
        Finally
            If ComPort IsNot Nothing Then ComPort.Close()

            'deFunctionStart = Nothing
            'deFunctionStop = Nothing
            'deFunctionMessage = Nothing

        End Try


    End Function

    Private Sub GetWeight()

        Try

            Dim PortName As String = Config.ComPortName

            ClearSerielPortBuffer()

            MSCOM = New IO.Ports.SerialPort(PortName)

            With MSCOM
                .BaudRate = Config.BaudRate
                .DataBits = Config.DataBits
                .Parity = System.IO.Ports.Parity.Odd
                .StopBits = StopBits.One
                .Handshake = Handshake.XOnXOff
                .DtrEnable = True
                .RtsEnable = True
                .ReceivedBytesThreshold = 16
                .ReadTimeout = 1000
            End With


            MSCOM.Open()
            If MSCOM.IsOpen Then
                MSCOM.Write(Chr(27) + "P")
            End If

            Do Until Config.FlagWeight = True
                Application.DoEvents()
            Loop

            Dim datastring As String
            datastring = "IDT-WEI-" & lbWeight.Text & RETURN_SPLITER & Environment.NewLine
            SendData(datastring)

        Catch ex As Exception
            'Not Found
            SendData("NFW" & RETURN_SPLITER & Environment.NewLine)
        Finally
            If ComPort IsNot Nothing Then ComPort.Close()
        End Try

    End Sub

    Private Sub MSCOM_DataReceived(sender As Object, e As IO.Ports.SerialDataReceivedEventArgs) Handles MSCOM.DataReceived
        Try

            Q.Enqueue(MSCOM.ReadExisting())
            Dim value As String = ""
            SyncLock Q
                While Q.Count > 0
                    lbWeight.Invoke(Sub()
                                        lbWeight.Text = ""
                                        If Config.WeightName = "GD603" Then
                                            lbWeight.Text = Mid(Q.Dequeue, 8).Trim
                                        Else
                                            lbWeight.Text = Mid(Q.Dequeue, 6, 5).Trim
                                        End If

                                        CreateLog(lbWeight.Text)

                                    End Sub)
                End While
            End SyncLock

        Catch ex As Exception

        End Try
    End Sub

    Private Sub ClearSerielPortBuffer()
        Try
            If MSCOM Is Nothing Then
                '-- DO Nothing --
            Else
                If MSCOM.IsOpen = True Then
                    MSCOM.DiscardInBuffer()
                    MSCOM.DiscardOutBuffer()
                Else
                    MSCOM.Open()
                    MSCOM.DiscardInBuffer()
                    MSCOM.DiscardOutBuffer()
                End If
                If MSCOM.IsOpen = True Then MSCOM.Close()
                MSCOM = Nothing
            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Function VPRODClientRunning() As Boolean
        ' check if VPROD client is running resutl false 
        Try
            Return (Process.GetProcessesByName(VPROD_CLIENT_NAME).Length > 0)

        Catch ex As Exception
            'Write_Log("VPRODClientRunning : " & ex.Message)
        End Try

    End Function

    Public Sub StartListening()
        ' init RFID reader

        'ctrlFingerPrint.Enabled = True
        Try
            If connectCount <= 2 Then
                'If ctrlFingerPrint.Enabled Then
                connectCount = 0
                ' Console.WriteLine("Command - Finger Control is activeted ")
                ' init socket
                bwListener = New BackgroundWorker()
                bwListener.WorkerSupportsCancellation = True
                bwListener.WorkerReportsProgress = True
                AddHandler bwListener.DoWork, AddressOf Initialize_Connection
                AddHandler bwListener.RunWorkerCompleted, AddressOf Initialize_Connection
                bwListener.RunWorkerAsync()

                'Dim thrd As New Thread(AddressOf Initialize_Connection)
                'thrd.Start()

                If Not Config.Visible Then
                    Me.WindowState = FormWindowState.Minimized
                End If
                'Else
                '    connectCount = connectCount + 1
                '    Console.WriteLine("Finger Control can not be activeted")
                '    StartListening()
                'End If
            Else
                Console.WriteLine("Unable to activate fingerprint reader!")
            End If
        Catch ex As Exception
            ' Write_Log("StartListening : " & ex.Message)
        End Try


    End Sub

    Private Sub OnRecievedData(ByVal ar As IAsyncResult)

        'On Error Resume Next
        'CMD-Int()#
        Dim CheckState As String = CheckCommand

        Dim so As frmConsole = CType(ar.AsyncState, frmConsole)
        Dim s As Socket = so.processSocket
        Dim thProcess As New Thread(AddressOf BeginProcess)


        ar.AsyncWaitHandle.WaitOne()

        Try
            If Not so.processSocket Is Nothing Then
                Dim SocErr As System.Net.Sockets.SocketError
                Dim read As Integer = 0
                If s.Connected = True Then              'ND14194 Add When Async return
                    read = s.EndReceive(ar, SocErr)     'ND14194 Add err
                    If SocErr = 0 Then                  'Nd14194 Check err
                        If read > 0 Then
                            so.sb.Append(Encoding.ASCII.GetString(so.rBuffer, 0, read))
                            If so.sb.ToString.Length > 2 AndAlso so.sb.ToString.Substring(so.sb.ToString.Length - 2) = "##" Then
                                If Me._Reading_Started Then
                                    If so.sb.ToString <> "" Then
                                        Command_Stop_Scaning(so.sb.ToString)
                                        Invoke(Sub()
                                                   HandlerNumber = 0
                                               End Sub)
                                    End If
                                Else
                                    HandlerNumber = 0

                                    If HandlerNumber = 0 Then
                                        Invoke(Sub()
                                                   HandlerNumber = so.Handle
                                               End Sub)

                                        Dim thrd As New Thread(AddressOf BeginProcess)
                                        thrd.Start()

                                        s.BeginReceive(so.rBuffer, 0, rBuffer.Length, 0, SocErr, New AsyncCallback(AddressOf OnRecievedData), so)
                                    Else
                                        If CheckState = "STT" Then ' For Stop Scanning --

                                            Dim thrd As New Thread(AddressOf BeginProcess)
                                            thrd.Start()

                                            s.BeginReceive(so.rBuffer, 0, rBuffer.Length, 0, SocErr, New AsyncCallback(AddressOf OnRecievedData), so)
                                        End If
                                    End If

                                End If
                            Else
                                s.BeginReceive(so.rBuffer, 0, rBuffer.Length, 0, SocErr, New AsyncCallback(AddressOf OnRecievedData), so)
                            End If
                        End If
                    Else
                        If s.Connected = True Then
                            s.EndAccept(ar)
                            s.EndConnect(ar)
                            s.Close()
                            s.Dispose()
                            s = Nothing

                            If so.Handle = HandlerNumber Then
                                HandlerNumber = 0
                            End If

                            'so.Result = Nothing

                        End If

                        Invoke(Sub()
                                   If so.Handle = HandlerNumber Then
                                       HandlerNumber = 0
                                   End If
                               End Sub)
                        ar.AsyncWaitHandle.Close()

                        'If SocErr = "10054" Then
                        '    Write_Log("OnRecievedData - An existing connection was forcibly closed by the remote host")
                        'Else
                        '    Write_Log("OnRecievedData - CallBackError : " & SocErr)
                        'End If

                    End If
                Else
                    ar.AsyncWaitHandle.Close()
                    Invoke(Sub()
                               If so.Handle = HandlerNumber Then
                                   HandlerNumber = 0
                               End If
                           End Sub)
                End If
            Else
                If s.Connected = True Then
                    s.EndAccept(ar)
                    s.EndConnect(ar)
                    s.Close()
                    s.Dispose()
                    s = Nothing
                End If
                ar.AsyncWaitHandle.Close()
                Invoke(Sub()
                           If so.Handle = HandlerNumber Then
                               HandlerNumber = 0
                           End If
                       End Sub)
            End If
        Catch ex As Exception
            ' Write_Log("OnRecievedData : " & ex.Message)
            GC.Collect()
        Finally
            'If Not thProcess Is Nothing Then
            '    thProcess.Abort()
            'End If
        End Try

    End Sub

    Private Overloads Sub Command_Stop_Scaning(ByVal cmdString As String)
        If cmdString.IndexOf("STP##") > -1 Then
            'RaiseEvent ChangeStatus("Handler " & Me._ClientNumber & " : command stop")
            Try

                ' NN to stop reading
                'Stop_Scanning()

                Me.sb = New StringBuilder
                SendData(RETURN_DATA)
                SendData(RETURN_STOP)
                Thread.Sleep(200)

                If Not Me.processSocket Is Nothing Then
                    Me.processSocket.Close()
                End If

                Me._Reading_Started = False
                Me._Reading = False

            Catch ex As Exception

            End Try

        End If
    End Sub

    Public Sub Initialize_Configuration()
        ' initial of configuration 
        Try
            Dim xmlConfig As New XmlDocument

            Dim i As Integer = 0
            Dim ii As Integer = 0

            'path default from xml
            xmlConfig.Load(CONFIGURATIONFILE)
            'ค่า config จาก xml
            Dim nodeList As XmlNodeList = xmlConfig.SelectNodes(CONFIG_SelectNodes)
            If nodeList.Count > 0 Then
                For Each node As XmlNode In nodeList
                    If node.Attributes.GetNamedItem("name").Value() = CONFIG_ComPortName Then
                        Config.ComPortName = node.Attributes("value").Value
                    End If
                    If node.Attributes.GetNamedItem("name").Value() = CONFIG_WeightName Then
                        Config.WeightName = node.Attributes("value").Value
                    End If
                    If node.Attributes.GetNamedItem("name").Value() = CONFIG_MaxClient Then
                        Config.MaxClient = node.Attributes("value").Value
                    End If
                    If node.Attributes.GetNamedItem("name").Value() = CONFIG_ListenPort Then
                        Config.ListenPort = node.Attributes("value").Value
                    End If
                    If node.Attributes.GetNamedItem("name").Value() = CONFIG_Visible Then
                        Config.Visible = node.Attributes("value").Value
                    End If
                    If node.Attributes.GetNamedItem("name").Value() = CONFIG_BaudRate Then
                        Config.BaudRate = node.Attributes("value").Value
                    End If
                    If node.Attributes.GetNamedItem("name").Value() = CONFIG_DataBits Then
                        Config.DataBits = node.Attributes("value").Value
                    End If
                Next

            Else
                MessageBox.Show("Not found file config.")
            End If
        Catch ex As Exception
            MessageBox.Show("Error load Program " & ex.Message)
        End Try

    End Sub

    Private Sub CreateLog(ByVal strdata As String)

        strdata = DateTime.Now.ToString & " : " & strdata
        On Error Resume Next
        Dim mydocpath As String = Application.StartupPath & "\Logfile"
        Dim file As System.IO.StreamWriter
        file = My.Computer.FileSystem.OpenTextFileWriter(mydocpath & "\LogScale.txt", True)
        file.WriteLine(strdata)
        file.Close()

    End Sub

    Private Sub ShowConsoleToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowConsoleToolStripMenuItem.Click
        Me.WindowState = FormWindowState.Normal
    End Sub

    Private Sub frmConsole_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            If ExitApp = True Or (Not VPRODClientRunning() And e.CloseReason <> CloseReason.UserClosing) Then
                ' aa
                'If Not IsNothing(ScanThread) Then
                '    ScanThread.Abort()
                '    ScanThread = Nothing
                'End If

                Me.NotifyIcon.Dispose()
                Application.Exit()
            Else
                If Not Config.Visible Then
                    Me.WindowState = FormWindowState.Minimized
                End If
                e.Cancel = True
            End If
        Catch ex As Exception
            'Write_Log("frmConsole_FormClosing : " & ex.Message)
        End Try

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Try

            Application.Exit()

        Catch ex As Exception

        End Try
    End Sub

    Private Sub lbWeight_TextChanged(sender As Object, e As EventArgs) Handles lbWeight.TextChanged
        If lbWeight.Text <> "" Then
            Config.FlagWeight = True
        End If
    End Sub

#End Region


End Class