﻿namespace Pancake.Core
{
    public interface ICloneable<T>
    {
        T Clone();
    }


    public interface ICopyable<T>
    {
        void Copy(T target);
    }
}