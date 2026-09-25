using System;

internal static class MenuPreviewEntry
{
    [STAThread]
    private static int Main(string[] args) => Quartz.Controls.ChromiumMenus.MenuPreview.Run(args);
}
