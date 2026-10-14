using Datinate.Shared.Dat;
using Datinate.Shared.DatGrouper;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class AddDatToProjectCmd : CommandBase<bool>
    {
        private readonly DatVO dat;
        private readonly COLLECTION_SET_ENUM collectionSetEnum;
        private readonly DAT_GROUP_TARGET_ENUM datGroupTargetEnum;
        private readonly DAT_GROUP_ENUM datGroupEnum;
        private readonly string? internalDescriptor;
        private readonly DatSubsetFilter? datSubsetFilter;

        public AddDatToProjectCmd(
            DatVO dat, 
            COLLECTION_SET_ENUM collectionSetEnum,
            DAT_GROUP_TARGET_ENUM datGroupTargetEnum, 
            DAT_GROUP_ENUM datGroupEnum, 
            string? internalDescriptor, 
            DatSubsetFilter? datSubsetFilter = null) 
        {
            this.dat = dat;
            this.collectionSetEnum = collectionSetEnum;
            this.datGroupTargetEnum = datGroupTargetEnum;
            this.datGroupEnum = datGroupEnum;
            this.internalDescriptor = internalDescriptor;
            this.datSubsetFilter = datSubsetFilter;
        }

        protected override async Task<bool> Run() 
        {
            if (base.ProjectLoaderMediator is { } && 
                ProjectLoaderMediator.IsProjectLoaded == false &&
                base.Shell is { })
            {
                _ = base.Shell.ShowMessageBox(
                    "Attention",
                    "Cannot proceed. Please Load or Create a DAT Grouper Project and try again.");

                await base.Execute(new SetDatGrouperFormVisibleCmd());

                return await Complete(false);
            }

            DatGrouperProjectEntry[] existingVOs =
                base.ProjectLoaderMediator?.GetAllDats() ?? new DatGrouperProjectEntry[] { };

            bool problemFound;

            for (int i = 0; i < existingVOs.Length; i++) 
            {
                var trialVO = existingVOs[i];

                // NOTE: No fullpath can be the same if even one vo is missing subset data:
                if (trialVO.DatFullpath.ToLower().Trim() == dat.DatFullpath.ToLower().Trim()) 
                {
                    problemFound = false;

                    if (trialVO.DatSubsetFilter == null || datSubsetFilter == null)
                        problemFound = true;

                    else if (trialVO.DatSubsetFilter.Equals(datSubsetFilter))
                        problemFound = true;

                    if (problemFound) 
                    {
                        _ = base.Shell?.ShowMessageBox(
                            "There was a Problem",
                            "Cannot proceed. This DAT File has already been added to the Project.");

                        return await Complete(false);
                    }
                }
            }

            DatGrouperProjectEntry datHeadlineVO = new DatGrouperProjectEntry(
                collectionSetEnum,
                dat.DatFullpath,
                string.Empty,
                string.Empty,
                datGroupEnum,
                string.Empty,
                datSubsetFilter,
                internalDescriptor,
                null,
                null,
                false);

            base.ProjectLoaderMediator?.AddDat(datHeadlineVO, datGroupTargetEnum);
            await base.Execute(new SetDatGrouperFormVisibleCmd());

            base.Shell?.SetAddToProjectFormVisible(false);

            return await Complete(true);
        }
    }
}
