﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ServiceModel;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Debugger.Target
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GameDebuggerHost : IGameDebuggerHost
    {
        private TaskCompletionSource<IGameDebuggerTarget> target = new TaskCompletionSource<IGameDebuggerTarget>();

        public event Action GameExited;

        public LoggerResult Log { get; private set; }

        public GameDebuggerHost(LoggerResult logger)
        {
            Log = logger;
        }

        public Task<IGameDebuggerTarget> Target
        {
            get { return target.Task; }
        }

        public void RegisterTarget()
        {
            target.TrySetResult(OperationContext.Current.GetCallbackChannel<IGameDebuggerTarget>());
        }

        public void OnGameExited()
        {
            GameExited?.Invoke();
        }

        public void OnLogMessage(SerializableLogMessage logMessage)
        {
            Log.Log(logMessage);
        }
    }
}