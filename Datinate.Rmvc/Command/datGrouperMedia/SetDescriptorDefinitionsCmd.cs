using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetDescriptorDefinitionsCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            var defs = DescriptorDefinitions.All;
            base.MediaAssignmentMediator?.SetDescriptorDefinitions(defs);
            base.RadioDatModel?.SetDescriptorDefinitions(defs);

            return await Complete();
        }
    }
}
