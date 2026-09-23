using System;
using System.Windows.Forms;
using Quartz;

internal static class TabOpeningChecks
{
    private static int _checks;
    private static void Equal<T>(T expected, T actual, string label)
    {
        _checks++;
        if (!Equals(expected, actual)) throw new Exception(label + ": expected " + expected + ", got " + actual);
    }

    [STAThread]
    private static void Main()
    {
        var current = TabOpenDisposition.CurrentTab;
        var foreground = TabOpenDisposition.NewForegroundTab;
        var background = TabOpenDisposition.NewBackgroundTab;
        var window = TabOpenDisposition.NewWindow;
        Equal(current, TabOpenPolicy.FromClick(MouseButtons.Left, Keys.None), "ordinary link");
        Equal(background, TabOpenPolicy.FromClick(MouseButtons.Middle, Keys.None), "middle link");
        Equal(background, TabOpenPolicy.FromClick(MouseButtons.Left, Keys.Control), "Ctrl link");
        Equal(foreground, TabOpenPolicy.FromClick(MouseButtons.Left, Keys.Control | Keys.Shift), "Ctrl Shift link");
        Equal(foreground, TabOpenPolicy.FromClick(MouseButtons.Middle, Keys.Shift), "Shift middle link");
        Equal(window, TabOpenPolicy.FromClick(MouseButtons.Left, Keys.Shift), "Shift link");
        Equal(TabOpenDisposition.SaveToDisk, TabOpenPolicy.FromClick(MouseButtons.Left, Keys.Alt), "Alt link");
        Equal(background, TabOpenPolicy.FromClick(MouseButtons.Left, Keys.Control | Keys.Alt), "Ctrl precedes Alt");
        Equal(window, TabOpenPolicy.FromClick(MouseButtons.Left, Keys.Shift | Keys.Alt), "Shift precedes Alt");
        Equal(foreground, TabOpenPolicy.FromClick(MouseButtons.Middle, Keys.Alt | Keys.Shift), "middle precedes Alt");
        Equal(false, TabOpenPolicy.ShouldActivate(background, false), "preserve existing selection");
        Equal(true, TabOpenPolicy.ShouldActivate(background, true), "first tab cannot remain inactive");
        Equal(true, TabOpenPolicy.ShouldActivate(foreground, false), "new and duplicate commands activate");
        Equal(current, TabOpenPolicy.FromAddressBar(Keys.None), "address Enter");
        Equal(current, TabOpenPolicy.FromAddressBar(Keys.Control), "address Ctrl Enter is not a tab modifier");
        Equal(foreground, TabOpenPolicy.FromAddressBar(Keys.Alt), "address Alt Enter");
        Equal(background, TabOpenPolicy.FromAddressBar(Keys.Alt | Keys.Shift), "address Alt Shift Enter");
        Equal(window, TabOpenPolicy.FromAddressBar(Keys.Shift), "address Shift Enter");

        // No HWND is created, so these checks exercise the actual gesture
        // correlation class without installing a native hook or showing a UI.
        using (var control = new Control())
        using (var input = new TabOpenGesture(control, () => true))
        {
            Equal(foreground, input.Take(true, false, 100), "target blank default");
            Equal(foreground, input.Take(false, false, 100), "allowed script window default");
            Equal(window, input.Take(false, true, 100), "script popup default");
            input.Record(MouseButtons.Middle, Keys.None, 100);
            Equal(background, input.Take(true, false, 120), "released middle button still captured");
            Equal(foreground, input.Take(true, false, 121), "gesture consumed only once");
            input.Record(MouseButtons.Left, Keys.Control | Keys.Shift, 100);
            Equal(foreground, input.Take(true, false, 120), "modifiers captured at input time");
            input.Record(MouseButtons.Middle, Keys.None, 100);
            Equal(foreground, input.Take(false, false, 120), "unrelated script cannot inherit background intent");
            input.Record(MouseButtons.Middle, Keys.None, 100);
            Equal(foreground, input.Take(true, false, 1101), "stale gesture expires");
            input.Record(MouseButtons.Left, Keys.Control, 100);
            input.Clear();
            Equal(foreground, input.Take(true, false, 120), "context menu or navigation clears gesture");
            input.Record(MouseButtons.Left, Keys.None, 100);
            Equal(foreground, input.Take(true, false, 120), "unmodified target blank activates");
            input.Record(MouseButtons.Left, Keys.Shift, 100);
            Equal(window, input.Take(true, false, 120), "Shift renderer open creates window");
            input.Record(MouseButtons.Middle, Keys.None, 100);
            Equal(background, input.Take(true, true, 120), "user tab intent overrides popup features");
            input.Record(MouseButtons.Left, Keys.Alt, 100);
            Equal(foreground, input.Take(true, false, 120), "script open does not become Alt download");
            input.Record(MouseButtons.Middle, Keys.None, int.MaxValue - 10);
            Equal(background, input.Take(true, false, int.MinValue + 10), "native clock wrap");
        }
        Console.WriteLine("PASS: " + _checks + " tab-opening policy and gesture checks");
    }
}
