﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public interface ITranslator : INotifyPropertyChanged
    {
        Guid Identify { get; set; }

        ServiceType Type { get; set; }

        IconType Icon { get; set; }

        bool IsEnabled { get; set; }

        string Name { get; set; }

        string Url { get; set; }

        bool AutoExecute { get; set; }

        TranslationResult Data { get; set; }

        BindingList<UserDefinePrompt> UserDefinePrompts { get; set; }

        /// <summary>
        /// 手动通知属性更新
        /// </summary>
        /// <param name="name"></param>
        void ManualPropChanged(params string[] name);

        string AppID { get; set; }

        string AppKey { get; set; }

        string? LangConverter(LangEnum lang);

        bool IsExecuting { get; set; }

        Task<TranslationResult> TranslateAsync(object request, CancellationToken token);

        Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token);

        ITranslator Clone();
    }

    public interface ITranslatorLlm : ITranslator
    { }
}
