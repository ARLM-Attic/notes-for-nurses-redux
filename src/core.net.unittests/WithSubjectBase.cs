﻿// -----------------------------------------------------------------------
// <copyright file="WithSubjectBase.cs">
// Copyright (c) 2013.
// </copyright>
// -----------------------------------------------------------------------
namespace core.net.tests
{
    using Machine.Fakes;

    public class WithSubjectBase<TSubject> : WithSubject<TSubject>
        where TSubject : class
    {
        private static bool initialized;

        static WithSubjectBase()
        {
            Initialize();
        }

        public static TType Resolve<TType>() where TType : class
        {
            if (!initialized)
            {
                Initialize();    
            }

            return TinyIoC.TinyIoCContainer.Current.Resolve<TType>();
        }

        private static void Initialize()
        {
            TinyIoC.TinyIoCContainer.Current.AutoRegister();
            initialized = true;
        }
    }
}