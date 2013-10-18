﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger {
    class NodeStackFrame {
        private int _lineNo;    // mutates on set next line
        private readonly string _frameName;
        private readonly int _frameId;
        private readonly int _startLine, _endLine;
        private int _argCount;
        private NodeEvaluationResult[] _variables;
        private readonly NodeThread _thread;
        private readonly NodeModule _module;

        public NodeStackFrame(NodeThread thread, NodeModule module, string frameName, int startLine, int endLine, int lineNo, int argCount, int frameId) {
            _thread = thread;
            _module = module;
            _frameName = frameName;
            _argCount = argCount;
            _lineNo = lineNo;
            _frameId = frameId;
            _startLine = startLine;
            _endLine = endLine;
        }

        /// <summary>
        /// The line nubmer where the current function/class/module starts
        /// </summary>
        public int StartLine {
            get {
                return _startLine;
            }
        }

        /// <summary>
        /// The line number where the current function/class/module ends.
        /// </summary>
        public int EndLine {
            get {
                return _endLine;
            }
        }

        public NodeThread Thread {
            get {
                return _thread;
            }
        }

        public int LineNo {
            get {
                return _lineNo;
            }
            set {
                _lineNo = value;
            }
        }

        public string FunctionName {
            get {
                return _frameName;
            }
        }

        public string FileName {
            get {
                return _module.FileName;
            }
        }

        public NodeModule Module {
            get {
                return _module;
            }
        }

        /// <summary>
        /// Gets the ID of the frame.  Frame 0 is the currently executing frame, 1 is the caller of the currently executing frame, etc...
        /// </summary>
        public int FrameId {
            get {
                return _frameId;
            }
        }

        internal void SetArgCount(int argCount) {
            _argCount = argCount;
        }

        internal void SetVariables(NodeEvaluationResult[] variables) {
            _variables = variables;
        }

        public IList<NodeEvaluationResult> Locals {
            get {
                NodeEvaluationResult[] res = new NodeEvaluationResult[_variables.Length - _argCount];
                for (int i = _argCount; i < _variables.Length; i++) {
                    res[i - _argCount] = _variables[i];
                }
                return res;
            }
        }

        public IList<NodeEvaluationResult> Parameters {
            get {
                NodeEvaluationResult[] res = new NodeEvaluationResult[_argCount];
                for (int i = 0; i < _argCount; i++) {
                    res[i] = _variables[i];
                }
                return res;
            }
        }

        /// <summary>
        /// Attempts to parse the given text.  Returns true if the text is a valid expression.  Returns false if the text is not
        /// a valid expression and assigns the error messages produced to errorMsg.
        /// </summary>
        public virtual bool TryParseText(string text, out string errorMsg) {
#if NEEDS_UPDATING
            CollectingErrorSink errorSink = new CollectingErrorSink();
            Parser parser = Parser.CreateParser(new StringReader(text), _thread.Process.LanguageVersion, new ParserOptions() { ErrorSink = errorSink });
            var ast = parser.ParseSingleStatement();
            if (errorSink.Errors.Count > 0) {
                StringBuilder msg = new StringBuilder();
                foreach (var error in errorSink.Errors) {
                    msg.Append(error.Message);
                    msg.Append(Environment.NewLine);
                }

                errorMsg = msg.ToString();
                return false;
            }
#endif

            errorMsg = null;
            return true;
        }

        /// <summary>
        /// Executes the given text against this stack frame.
        /// </summary>
        /// <param name="text"></param>
        public virtual void ExecuteText(string text, Action<NodeEvaluationResult> completion) {
            _thread.Process.ExecuteText(text, this, completion);
        }

        /// <summary>
        /// Sets the line number that this current frame is executing.  Returns true
        /// if the line was successfully set or false if the line number cannot be changed
        /// to this line.
        /// </summary>
        public bool SetLineNumber(int lineNo) {
            return _thread.Process.SetLineNumber(this, lineNo);
        }
    }
}