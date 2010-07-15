//
// WebSource.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright 2010 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Mono.Unix;

using Hyena;

using Banshee.Sources;
using Banshee.Sources.Gui;
using Banshee.WebBrowser;

namespace Banshee.WebSource
{
    public abstract class WebSource : Source
    {
        private WebSourceContents source_contents;

        public WebSource (string name, int order, string id) : base (name, name, order, id)
        {
            TypeUniqueId = id;
            Properties.Set<bool> ("Nereid.SourceContents.HeaderVisible", false);
        }

        public override void Activate ()
        {
            if (source_contents == null) {
                Properties.Set<ISourceContents> ("Nereid.SourceContents",
                    source_contents = new WebSourceContents (this, GetWidget ()));
            }

            base.Activate ();
        }

        protected abstract Gtk.Widget GetWidget ();

        public override int Count {
            get { return 0; }
        }

        private class WebSourceContents : ISourceContents
        {
            private WebSource source;
            private Gtk.Widget widget;

            public WebSourceContents (WebSource source, Gtk.Widget widget)
            {
                this.source = source;
                this.widget = widget;
            }

            public bool SetSource (ISource source)
            {
                return true;
            }

            public void ResetSource ()
            {
            }

            public Gtk.Widget Widget {
                get { return widget; }
            }

            public ISource Source {
                get { return source; }
            }
        }
    }
}