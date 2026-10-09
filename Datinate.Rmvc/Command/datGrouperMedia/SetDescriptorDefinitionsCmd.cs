using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class SetDescriptorDefinitionsCmd : RCommand
    {
        protected override void Run()
        {
            var defs = DescriptorDefinitions.All;
            Facade.Instance?.MediaAssignmentMediator?.SetDescriptorDefinitions(defs);
            Facade.Instance?.RadioDatModel?.SetDescriptorDefinitions(defs);
        }
    }
}
