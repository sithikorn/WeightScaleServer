<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmConsole
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmConsole))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lbWeight = New System.Windows.Forms.Label()
        Me.MSCOM = New System.IO.Ports.SerialPort(Me.components)
        Me.NotifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.ContextMenuRClick = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ShowConsoleToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ContextMenuRClick.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(48, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Weight :"
        '
        'lbWeight
        '
        Me.lbWeight.AutoSize = True
        Me.lbWeight.Location = New System.Drawing.Point(66, 9)
        Me.lbWeight.Name = "lbWeight"
        Me.lbWeight.Size = New System.Drawing.Size(0, 13)
        Me.lbWeight.TabIndex = 1
        '
        'MSCOM
        '
        '
        'NotifyIcon
        '
        Me.NotifyIcon.ContextMenuStrip = Me.ContextMenuRClick
        Me.NotifyIcon.Icon = CType(resources.GetObject("NotifyIcon.Icon"), System.Drawing.Icon)
        Me.NotifyIcon.Text = "Weight Reader"
        Me.NotifyIcon.Visible = True
        '
        'ContextMenuRClick
        '
        Me.ContextMenuRClick.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ShowConsoleToolStripMenuItem, Me.ExitToolStripMenuItem})
        Me.ContextMenuRClick.Name = "ContextMenuRClick"
        Me.ContextMenuRClick.Size = New System.Drawing.Size(150, 48)
        '
        'ShowConsoleToolStripMenuItem
        '
        Me.ShowConsoleToolStripMenuItem.Name = "ShowConsoleToolStripMenuItem"
        Me.ShowConsoleToolStripMenuItem.Size = New System.Drawing.Size(149, 22)
        Me.ShowConsoleToolStripMenuItem.Text = "Show Console"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(149, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'frmConsole
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(164, 29)
        Me.ControlBox = False
        Me.Controls.Add(Me.lbWeight)
        Me.Controls.Add(Me.Label1)
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmConsole"
        Me.ShowInTaskbar = False
        Me.Text = "WeightScaleServer V64.01"
        Me.ContextMenuRClick.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents lbWeight As Label
    Friend WithEvents MSCOM As IO.Ports.SerialPort
    Friend WithEvents NotifyIcon As NotifyIcon
    Friend WithEvents ContextMenuRClick As ContextMenuStrip
    Friend WithEvents ShowConsoleToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem
End Class
