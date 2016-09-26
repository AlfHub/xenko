﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// A <see cref="Command"/> that reads and/or writes to the index file.
    /// </summary>
    public abstract class IndexFileCommand : Command
    {
        private BuildTransaction buildTransaction;

        public override void PreCommand(ICommandContext commandContext)
        {
            base.PreCommand(commandContext);

            buildTransaction = MicrothreadLocalDatabases.CreateTransaction(commandContext.GetOutputObjectsGroups());
            MicrothreadLocalDatabases.MountDatabase(buildTransaction);
        }

        public override void PostCommand(ICommandContext commandContext, ResultStatus status)
        {
            base.PostCommand(commandContext, status);

            if (status == ResultStatus.Successful)
            {
                // Save list of newly changed URLs in CommandResult.OutputObjects
                foreach (var entry in buildTransaction.GetTransactionIdMap())
                {
                    commandContext.RegisterOutput(entry.Key, entry.Value);
                }

                // Note: In case of remote process, the remote process will save the index map.
                // Alternative would be to not save it and just forward results to the master builder who would commit results locally.
                // Not sure which is the best.
                //
                // Anyway, current approach should be OK for now since the index map is "process-safe" (as long as we load new values as necessary).
                //contentIndexMap.Save();
            }

            MicrothreadLocalDatabases.UnmountDatabase();
        }
    }
}
