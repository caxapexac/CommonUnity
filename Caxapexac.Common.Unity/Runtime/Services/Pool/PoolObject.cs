// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/Leopotam/LeopotamGroupLibraryUnity
// Copyright (c) 2012-2019 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using UnityEngine;


namespace Caxapexac.Common.Sharp.Runtime.Services.Pool
{
    public interface IPoolObject
    {
        /// <summary>
        /// Pool container - spawner of this instance, should be set only once.
        /// </summary>
        /// <value>The pool container.</value>
        PoolContainer PoolContainer { get; set; }

        /// <summary>
        /// Transform of spawned instance, can be null if you dont need it.
        /// </summary>
        Transform PoolTransform { get; }

        /// <summary>
        /// Recycle this instance.
        /// </summary>
        /// <param name="checkForDoubleRecycle">Check if instance already was recycled. Use false for performance boost.</param>
        void PoolRecycle(bool checkForDoubleRecycle = true);
    }


    /// <summary>
    /// Helper for PoolContainer.
    /// </summary>
    public class PoolObject : MonoBehaviour, IPoolObject
    {
        private PoolContainer _container;

        public virtual PoolContainer PoolContainer
        {
            get => _container;
            set => _container = value;
        }

        public virtual Transform PoolTransform
        {
            get => transform;
        }


        public virtual void PoolRecycle(bool checkDoubleRecycles = true)
        {
            if ((object)_container != null)
            {
                _container.Recycle(this, checkDoubleRecycles);
            }
        }
    }
}