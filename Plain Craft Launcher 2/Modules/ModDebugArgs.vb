Imports System.Globalization

'开发者本地调试 flag 解析（在 Application_Startup 早期调用）
'所有覆写默认禁用，必须显式 --dev-mode=true 才生效；--dev-mode=false 显式禁用。
'
'  --dev-mode=<true|false>             总开关（默认 false）
'  --dev-branch=<v>                     ModSecret.VersionBranchMain
'  --dev-reg-folder=<v>                 ModSecret.RegFolder
'  --dev-no-restrict-mc-download        ModSecret.RestrictMcDownload = False
'  --dev-color-hue=<0..360>             ModSecret.ColorHue
'  --dev-color-sat=<0..100>             ModSecret.ColorSat
'  --dev-color-light=<-100..100>        ModSecret.ColorLightAdjust
'  --dev-color-topbar-delta=<-180..180> ModSecret.ColorHueTopbarDelta
'  --dev-theme=<-1..999>                ModSecret.ThemeNow
'  --dev-theme-dont-click=<0..999>      ModSecret.ThemeDontClick
'  --dev-server-config-timeout=<ms>     ModSecret.ServerLoader.ReloadTimeout
'  --dev-anim-fps=<0..240>              ModAnimation.AniFPS
'  --dev-lang=<id>                      ModBase.Lang
'  --dev-net-thread-count=<1..128>      ModNet.NetTaskThreadCount
'  --dev-show-hidden                    PageSelectRight.ShowHidden = True（默认初值）
'  --dev-no-hover-anim                  MyListItem.IsMouseOverAnimationEnabled = False（默认初值）
'  --dev-no-save-size                   FormMain.IsSizeSaveable = False（加载后应用）

Public Module ModDebugArgs

    Private _enabled As Boolean = False '默认禁用，必须显式 --dev-mode=true
    Private _args As String()

    '供 PageSelectRight / MyListItem 等实例字段初始化读取的 Shared 默认值
    Public ShowHiddenDefault As Boolean = False
    Public ListItemHoverDefault As Boolean = True

    Public Sub Apply(Args As String())
        _args = Args
        If Args Is Nothing OrElse Args.Length = 0 Then Return
        '解析 --dev-mode 总开关（顺序无关，多次出现时最后一次生效）
        For Each Arg In Args
            Select Case Arg
                Case "--dev-mode=true", "--dev-mode=1", "--dev-mode=yes"
                    _enabled = True
                Case "--dev-mode=false", "--dev-mode=0", "--dev-mode=no"
                    _enabled = False
            End Select
        Next
        '应用其他 flag
        For Each Arg In Args
            Try : ApplyOne(Arg) : Catch : End Try
        Next
    End Sub

    Private Sub ApplyOne(Arg As String)
        If Not _enabled Then Exit Sub

        Select Case True
            Case Arg.StartsWith("--dev-mode=")
                '已处理
            Case Arg.StartsWith("--dev-branch=")
                ModSecret.VersionBranchMain = After(Arg, "--dev-branch=")
            Case Arg.StartsWith("--dev-reg-folder=")
                ModSecret.RegFolder = After(Arg, "--dev-reg-folder=")
            Case Arg = "--dev-no-restrict-mc-download"
                ModSecret.RestrictMcDownload = False
            Case Arg.StartsWith("--dev-color-hue=")
                ModSecret.ColorHue = Clamp(After(Arg, "--dev-color-hue="), 0, 360)
            Case Arg.StartsWith("--dev-color-sat=")
                ModSecret.ColorSat = Clamp(After(Arg, "--dev-color-sat="), 0, 100)
            Case Arg.StartsWith("--dev-color-light=")
                ModSecret.ColorLightAdjust = Clamp(After(Arg, "--dev-color-light="), -100, 100)
            Case Arg.StartsWith("--dev-color-topbar-delta=")
                ModSecret.ColorHueTopbarDelta = Clamp(After(Arg, "--dev-color-topbar-delta="), -180, 180)
            Case Arg.StartsWith("--dev-theme=")
                ModSecret.ThemeNow = Clamp(After(Arg, "--dev-theme="), -1, 999)
            Case Arg.StartsWith("--dev-theme-dont-click=")
                ModSecret.ThemeDontClick = Clamp(After(Arg, "--dev-theme-dont-click="), 0, 999)
            Case Arg.StartsWith("--dev-server-config-timeout=")
                Dim Ms = Clamp(After(Arg, "--dev-server-config-timeout="), 1000, Integer.MaxValue)
                If ModSecret.ServerLoader IsNot Nothing Then ModSecret.ServerLoader.ReloadTimeout = Ms
            Case Arg.StartsWith("--dev-anim-fps=")
                ModAnimation.AniFPS = Clamp(After(Arg, "--dev-anim-fps="), 0, 240)
            Case Arg.StartsWith("--dev-lang=")
                ModBase.Lang = After(Arg, "--dev-lang=")
            Case Arg.StartsWith("--dev-net-thread-count=")
                ModNet.NetTaskThreadCount = Clamp(After(Arg, "--dev-net-thread-count="), 1, 128)
            Case Arg = "--dev-show-hidden"
                ShowHiddenDefault = True
            Case Arg = "--dev-no-hover-anim"
                ListItemHoverDefault = False
        End Select
    End Sub

    '部分实例字段需在 FormMain 加载后赋值——由 FormMain.xaml.vb FormMain_Loaded 调用
    Public Sub ApplyFormInstanceFields(F As FormMain)
        If Not _enabled Then Exit Sub
        If _args Is Nothing Then Exit Sub
        For Each Arg In _args
            Try
                Select Case True
                    Case Arg = "--dev-no-save-size"
                        F.IsSizeSaveable = False
                End Select
            Catch
            End Try
        Next
    End Sub

    Private Function After(Arg As String, Prefix As String) As String
        Return Arg.Substring(Prefix.Length)
    End Function

    Private Function Clamp(Raw As String, MinVal As Integer, MaxVal As Integer) As Integer
        Dim V As Integer
        If Integer.TryParse(Raw, NumberStyles.Integer, CultureInfo.InvariantCulture, V) Then
            Return Math.Max(MinVal, Math.Min(MaxVal, V))
        End If
        Return MinVal
    End Function

End Module
