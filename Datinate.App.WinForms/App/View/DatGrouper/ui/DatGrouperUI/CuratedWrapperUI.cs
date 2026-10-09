namespace datinate.app
{
    public class CuratedWrapperUI : AutomatedWrapperUI
    {
        override protected DatGrouperUiBase CreatePrimary()
            => new CuratedUI();

        override protected DatGrouperUiBase CreateSurrogate()
            => new CuratedUI();
    }
}
