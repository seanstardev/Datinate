namespace Datinate.App.WinForms.View.UI.DatGrouper
{
    public class CuratedWrapperUI : AutomatedWrapperUI
    {
        override protected DatGrouperUiBase CreatePrimary()
            => new CuratedUI();

        override protected DatGrouperUiBase CreateSurrogate()
            => new CuratedUI();
    }
}
