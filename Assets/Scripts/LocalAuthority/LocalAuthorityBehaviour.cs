using LocalAuthority.Message;
using UnityEngine.Networking;

namespace LocalAuthority
{
    public abstract class LocalAuthorityBehaviour : NetworkBehaviour
    {
        protected readonly CommandHistory CmdHistory = new CommandHistory();


        /// <summary>
        /// Invoke a "command" on the server.
        /// </summary>
        /// <returns>True if the command was sent.</returns>
        protected bool SendCommand(short msgType, MessageBase msg)
        {
            return NetworkManager.singleton.client.Send(msgType, msg);
        }

        /// <summary>
        /// Return a new <see cref="CommandRecordMessage"/> initialized with 'netId' and 'cmdRecord'.
        /// </summary>
        protected T NewMessage<T>() where T : CommandRecordMessage, new()
        {
            var record = CmdHistory.NewRecord();
            var msg = new T();
            msg.netId = netId;
            msg.cmdRecord = record;
            return msg;
        }

        /// <summary>
        /// Fill this with calls to:
        ///     RegisterCallback(CustomMessageType, SomeCallback)
        /// </summary>
        protected abstract void RegisterCallbacks();

        /// <summary>
        /// Register a "command".
        /// </summary>
        protected void RegisterCallback(short msgType, NetworkMessageDelegate callback)
        {
            NetworkServer.RegisterHandler(msgType, callback);
        }

        protected virtual void Awake()
        {
            RegisterCallbacks();
        }
    }
}
