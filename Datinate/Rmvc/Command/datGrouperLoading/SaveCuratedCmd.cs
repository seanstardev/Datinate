using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SaveCuratedCmd : RCommandAsync 
    {
        protected override Task RunAsync()
        {

            RadioDatModel? radioDatModel = Facade.Instance?.RadioDatModel;
            DatGrouperModel? datGrouperModel = Facade.Instance?.DatGrouperModel;
            CuratedDatProxy? curatedDatProxy = Facade.Instance?.CuratedDatProxy;

            if (radioDatModel == null || datGrouperModel == null || curatedDatProxy == null) 
                return Task.CompletedTask;
            
            if (string.IsNullOrWhiteSpace(radioDatModel.ProjectName) || radioDatModel.DatMeta == null)
                return Task.CompletedTask;

            base.ExecuteCommand(new ShowProgressCmd("Saving Project", 1, 2));

            var familiesWithMedia = radioDatModel.CreateExportCollection(
                datGrouperModel.CuratedFamilies);

            curatedDatProxy?.SaveCurated(
                radioDatModel.ProjectName,
                familiesWithMedia,
                radioDatModel.SourceIdContentDictionary,
                radioDatModel.CreateSourceDatExportSnapshot(),
                radioDatModel.DatMeta);

            base.ExecuteCommand(new ClearProgressCmd());
            
            return Task.CompletedTask;
        }

        public static bool ContainsEntry(DatVO dat, string entryName)
        {
            return dat.Entries.FirstOrDefault(e => e.Name == entryName) != null;
        }
    }
}
