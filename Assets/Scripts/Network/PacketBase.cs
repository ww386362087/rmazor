﻿using System;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Network
{
    public abstract class PacketBase : IPacket
    {
        #region public properties
        
        public abstract string Id { get; }
        public abstract string Url { get; }
        public virtual string Method => "POST";
        public virtual bool OnlyOne => false;
        public virtual bool Resend => false;
        public bool IsDone { get; set; }
        public object Request { get; }
        public string ResponseRaw { get; private set; }
        
        public ErrorResponseArgs ErrorMessage {
            get
            {
                if (m_ErrorMessage == null)
                    return new ErrorResponseArgs
                    {
                        Id = 500,
                        Message = "Internal server error"
                    };
                return m_ErrorMessage;
            }
        }
        
        public long ResponseCode { get; set; }

        #endregion
        
        #region nonpublic members

        private ErrorResponseArgs m_ErrorMessage;
        private Action m_Success;
        private Action m_Fail;
        private Action m_Cancel;
        
        #endregion
        
        #region constructor

        protected PacketBase(object _Request)
        {
            Request = _Request;
        }

        #endregion
        
        #region nonpublic methods
        
        private void InvokeSuccess()
        {
            try
            {
                m_Success?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnSuccess error: {e.Message};\n StackTrace: {e.StackTrace}");
            }
        }

        private void InvokeFail()
        {
            try
            {
                m_Fail?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnFail error: {e.Message};\n StackTrace: {e.StackTrace}");
            }
        }

        private void InvokeCancel()
        {
            try
            {
                m_Cancel?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnCancel error: {e.Message};\n StackTrace: {e.StackTrace}");
            }
        }
        
        #endregion
        
        #region public methods

        public virtual IPacket OnSuccess(Action _Action)
        {
            m_Success = _Action;
            return this;
        }

        public virtual IPacket OnFail(Action _Action)
        {
            m_Fail = _Action;
            return this;
        }

        public virtual IPacket OnCancel(Action _Action)
        {
            m_Cancel = _Action;
            return this;
        }
        
        public virtual void DeserializeResponse(string _Json)
        {
            ResponseRaw = _Json;
            try
            {
                if (!CommonUtils.IsInRange(ResponseCode, 200, 299))
                    m_ErrorMessage = JsonConvert.DeserializeObject<ErrorResponseArgs>(_Json);
            }
            catch (JsonReaderException)
            {
                Debug.LogError(ResponseRaw);
                throw;
            }
            
            
            if (CommonUtils.IsInRange(ResponseCode, 200, 299))
                InvokeSuccess();
            else if (CommonUtils.IsInRange(ResponseCode, 300, 399))
                InvokeCancel();
            else if (CommonUtils.IsInRange(ResponseCode, 400, 599) || ResponseCode == 0)
                InvokeFail();

            IsDone = true;
        }
        
        #endregion
    }
}