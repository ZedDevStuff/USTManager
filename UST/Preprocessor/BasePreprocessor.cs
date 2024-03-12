using System.Collections.Generic;
using UnityEngine;

namespace USTManager.Preprocessor
{
    public abstract class BasePreprocessor
    {
        public abstract string Name { get; protected set; }
        public abstract string Version { get; protected set; }
        public abstract string Format { get; protected set; }

        public abstract void Apply(IEnumerable<AudioSource> sources);
    }
}