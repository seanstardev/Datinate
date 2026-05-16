using datinate.app;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    // TODO: Placeholder until I figure out what to do with descriptors
    public class SetDescriptorDefinitionsCmd : RCommand
    {
        protected override void Run()
        {
            var defs = DescriptorChipUtil.DescriptorDefinitions;
            Facade.Instance?.MediaAssignmentMediator?.SetDescriptorDefinitions(defs);
            Facade.Instance?.RadioDatModel?.SetDescriptorDefinitions(defs);
        }
    }
}
