﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EarTrumpet.Models;

namespace EarTrumpet.Services
{
    public class AudioSessionService : IAudioSessionService
    {
        static class Interop
        {
            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int RefreshAudioSessions();

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int GetAudioSessionCount();

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int GetAudioSessions(ref IntPtr sessions);

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int SetAudioSessionVolume(uint sessionId, float volume);

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int SetAudioSessionMute(uint sessionId, [MarshalAs(UnmanagedType.I1)] bool isMuted);

        }

        public IEnumerable<AudioSessionModel> GetAudioSessions()
        {
            Interop.RefreshAudioSessions();

            var sessionCount = Interop.GetAudioSessionCount();
            var sessions = new List<AudioSessionModel>();

            var rawSessionsPtr = IntPtr.Zero;
            Interop.GetAudioSessions(ref rawSessionsPtr);

            var sizeOfAudioSessionStruct = Marshal.SizeOf(typeof(InteropAudioSessionModel));
            for (var i = 0; i < sessionCount; i++)
            {
                var window = new IntPtr(rawSessionsPtr.ToInt64() + (sizeOfAudioSessionStruct * i));

                var session = (InteropAudioSessionModel)Marshal.PtrToStructure(window, typeof(InteropAudioSessionModel));
                sessions.Add(new AudioSessionModel()
                {
                    DeviceId = session.DeviceId,
                    BackgroundColor = session.BackgroundColor,
                    DisplayName = session.DisplayName,
                    GroupingId = session.GroupingId,
                    IconPath = session.IconPath,
                    IsDesktop = session.IsDesktop,
                    IsMuted = session.IsMuted,
                    ProcessId = session.ProcessId,
                    SessionId = session.SessionId,
                    Volume = session.Volume
                });
            }
            return sessions;
        }

        public IEnumerable<EarTrumpetAudioSessionModelGroup> GetAudioSessionGroups()
        {
            return GetAudioSessions().GroupBy(
                x => x.GroupingId,
                x => x, (key, result) => new EarTrumpetAudioSessionModelGroup(result.ToList()));
        }

        public void SetAudioSessionVolume(uint sessionId, float volume)
        {
            Interop.SetAudioSessionVolume(sessionId, volume);
        }

        public void SetAudioSessionMute(uint sessionId, bool isMuted)
        {
            Interop.SetAudioSessionMute(sessionId, isMuted);
        }
    }
}