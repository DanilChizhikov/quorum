using System;
using UnityEngine.LowLevel;

namespace DTech.UtilityAI
{
    internal static class PlayerLoopExtensions
    {
        public delegate void ModifyPlayerLoopAction(ref PlayerLoopSystem loopSystem);
        
        public static void ModifyPlayerLoop(ModifyPlayerLoopAction action)
        {
            PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
            action?.Invoke(ref loopSystem);
            PlayerLoop.SetPlayerLoop(loopSystem);
        }
        
        public static ref PlayerLoopSystem GetSystem<TSystem>(ref this PlayerLoopSystem loopSystem)
        {
            Type systemType = typeof(TSystem);
            for (int i = loopSystem.subSystemList.Length - 1; i >= 0; i--)
            {
                ref PlayerLoopSystem system = ref loopSystem.subSystemList[i];
                if (system.type == systemType)
                {
                    return ref system;
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        public static void AddSystem(ref this PlayerLoopSystem loopSystem, Type systemType, PlayerLoopSystem.UpdateFunction update)
        {
            loopSystem.subSystemList = loopSystem.subSystemList.Add(new PlayerLoopSystem
                    {
                            type = systemType,
                            updateDelegate = update,
                    });
        }

        public static void RemoveSystem(ref this PlayerLoopSystem loopSystem, Type systemType, bool recursive = true)
        {
            if (loopSystem.subSystemList == null || loopSystem.subSystemList.Length <= 0)
            {
                return;
            }
            
            for (int i = loopSystem.subSystemList.Length - 1; i >= 0; i--)
            {
                ref PlayerLoopSystem system = ref loopSystem.subSystemList[i];
                if (system.type == systemType)
                {
                    loopSystem.subSystemList = loopSystem.subSystemList.RemoteAt(i);
                }
                else if (recursive)
                {
                    system.RemoveSystem(systemType);
                }
            }
        }

        private static T[] Add<T>(this T[] source, T element)
        {
            if (source == null || source.Length <= 0)
            {
                source = new T[1];
            }
            else
            {
                Array.Resize(ref source, source.Length + 1);
            }

            source[^1] = element;
            return source;
        }

        private static T[] RemoteAt<T>(this T[] source, int index)
        {
            if (source == null || index >= source.Length || index < 0)
            {
                return source;
            }
            
            var nextArray = new T[source.Length - 1];
            if (index > 0)
            {
                Array.Copy(source, 0, nextArray, 0, index);
            }

            if (index < nextArray.Length)
            {
                Array.Copy(source, index + 1, nextArray, index, source.Length - (index + 1));
            }

            return nextArray;
        }
    }
}