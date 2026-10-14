using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SaveCuratedCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            if (base.RadioDatModel == null || base.DatGrouperModel == null || base.CuratedDatProxy == null) 
                return await Complete();
            
            if (base.RadioDatModel == null ||
                base.RadioDatModel.ProjectName == null ||
                string.IsNullOrWhiteSpace(RadioDatModel.ProjectName) ||
                base.RadioDatModel.DatMeta == null)
                return await Complete();

            await base.Execute(new ShowProgressCmd("Saving Project", 1, 2));

            var familiesWithMedia = base.RadioDatModel.CreateExportCollection(
                DatGrouperModel.CuratedFamilies);

            base.CuratedDatProxy?.SaveCurated(
                base.RadioDatModel.ProjectName,
                familiesWithMedia,
                base.RadioDatModel.SourceIdContentDictionary,
                base.RadioDatModel.CreateSourceDatExportSnapshot(),
                base.RadioDatModel.DatMeta);

            await base.Execute(new ClearProgressCmd());

            return await Complete();
        }
    }
}
