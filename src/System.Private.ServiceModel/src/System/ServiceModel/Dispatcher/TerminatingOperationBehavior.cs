// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.Reflection;

namespace System.ServiceModel.Dispatcher
{
    internal class TerminatingOperationBehavior
    {
        static void AbortChannel(object state)
        {
            ((IChannel)state).Abort();
        }

        public static TerminatingOperationBehavior CreateIfNecessary(DispatchRuntime dispatch)
        {
            if (IsTerminatingOperationBehaviorNeeded(dispatch))
            {
                return new TerminatingOperationBehavior();
            }
            else
            {
                return null;
            }
        }

        static bool IsTerminatingOperationBehaviorNeeded(DispatchRuntime dispatch)
        {
            for (int i = 0; i < dispatch.Operations.Count; i++)
            {
                DispatchOperation operation = dispatch.Operations[i];

                if (operation.IsTerminating)
                {
                    return true;
                }
            }

            return false;
        }

        internal void AfterReply(ref MessageRpc rpc)
        {
            if (rpc.Operation.IsTerminating && rpc.Channel.HasSession)
            {
                IOThreadTimer timer = new IOThreadTimer(new Action<object>(TerminatingOperationBehavior.AbortChannel),
                                                        rpc.Channel.Binder.Channel, false);

                timer.Set(rpc.Channel.CloseTimeout);
            }
        }

        internal static void AfterReply(ref ProxyRpc rpc)
        {
            if (rpc.Operation.IsTerminating && rpc.Channel.HasSession)
            {
                IChannel sessionChannel = rpc.Channel.Binder.Channel;
                rpc.Channel.Close(rpc.TimeoutHelper.RemainingTime());
            }
        }
    }
}
