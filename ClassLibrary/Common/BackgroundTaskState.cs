﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

namespace ClassLibrary.Common
{
    /// <summary>
    /// Indicates the state of the background task.
    /// </summary>
    /// <remarks>
    /// State is persisted to the application settings store so that the foreground
    /// process can discover and respond to the current state of the background task.
    /// </remarks>
    public enum BackgroundTaskState
    {
        Unknown,
        Started,
        Running,
        Canceled
    }
}
