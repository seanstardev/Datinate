using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    // TODO: Placeholder until I figure out what to do with descriptors
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
