using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SaveCuratedCmd : RCommand 
    {
        protected override void Run() 
        {

            RadioDatModel? radioDatModel = Facade.Instance?.RadioDatModel;
            DatGrouperModel? datGrouperModel = Facade.Instance?.DatGrouperModel;
            CuratedDatProxy? curatedDatProxy = Facade.Instance?.CuratedDatProxy;

            if (radioDatModel == null || datGrouperModel == null || curatedDatProxy == null) 
                return;
            
            if (string.IsNullOrWhiteSpace(radioDatModel.ProjectName) || radioDatModel.DatMeta == null)
                return;

            var familiesWithMedia = radioDatModel.CreateExportCollection(
                datGrouperModel.CuratedFamilies);

            curatedDatProxy?.SaveCurated(
                radioDatModel.ProjectName,
                familiesWithMedia,
                radioDatModel.SourceIdContentDictionary,
                radioDatModel.CreateSourceDatExportSnapshot(),
                radioDatModel.DatMeta);
        }

        public static bool ContainsEntry(DatVO dat, string entryName)
        {
            return dat.Entries.FirstOrDefault(e => e.Name == entryName) != null;
        }
    }
}
