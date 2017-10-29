using LocalAuthority.Message;
using TabletopCardCompanion;

namespace LocalAuthority
{
    public class CommandHistory
    {
        public CommandRecord NewRecord()
        {
            return new CommandRecord(PlayerInfo.LocalPlayer.netId, 0u);
        }
    }
}
