//
// ColumnCellChannel.cs
//
// Authors:
//   Gabriel Burt <gburt@novell.com>
//   Mike Urbanski <michael.c.urbanski@gmail.com>
//
// Copyright (C) 2009 Michael C. Urbanski
// Copyright (C) 2008 Novell, Inc.
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
using Gtk;
using Cairo;

using Mono.Unix;

using Hyena.Gui;
using Hyena.Gui.Theming;
using Hyena.Data.Gui;

using Banshee.Gui;
using Banshee.ServiceStack;
using Banshee.Collection.Gui;

using Banshee.Paas.Data;

namespace Banshee.Paas.Gui
{
    public class ColumnCellChannel : ColumnCell, IColumnCellDataHelper
    {
        private static int image_spacing = 4;
        private static int image_size = 48;
        
        // TODO replace this w/ new icon installation etc
        private static ImageSurface default_cover_image = new PixbufImageSurface (
            IconThemeUtils.LoadIcon (image_size, "podcast")
        );

        private static ImageSurface updating_image = PixbufImageSurface.Create (
            IconThemeUtils.LoadIcon (image_size, Stock.Refresh), true
        );

        private ArtworkManager artwork_manager;

        public ColumnCellDataHelper DataHelper { get; set; }

        public ColumnCellChannel () : base (null, true)
        {
            artwork_manager = ServiceManager.Get<ArtworkManager> ();
        }
    
        public override void Render (CellContext context, StateType state, double cellWidth, double cellHeight)
        {
            if (BoundObject == null) {
                return;
            }
            
            if (!(BoundObject is PaasChannel)) {
                throw new InvalidCastException("ColumnCellChannel can only bind to PaasChannel objects");
            }
            
            PaasChannel channel = (PaasChannel)BoundObject;
            
            bool disable_border = false;
            // remove
            artwork_manager.ToString ();
            // remove

            ImageSurface image = null;/*
            artwork_manager == null ? null
                : artwork_manager.LookupScaleSurface (PodcastService.ArtworkIdFor (feed), image_size, true);
            */
            bool waiting = false;
            
            if (DataHelper != null) {
                switch ((ChannelUpdateStatus)DataHelper (this, channel)) {
                case ChannelUpdateStatus.Waiting:
                    waiting = true;
                    goto case ChannelUpdateStatus.Updating;
                case ChannelUpdateStatus.Updating:
                    image = updating_image;
                    disable_border = true;
                    break;
                }    
            }
            
            if (image == null) {
                image = default_cover_image;
                disable_border = true;
            }
            
            // int image_render_size = is_default ? image.Height : (int)cellHeight - 8;
            int image_render_size = image_size;
            int x = image_spacing;
            int y = ((int)cellHeight - image_render_size) / 2;

            ArtworkRenderer.RenderThumbnail (context.Context, image, false, x, y,
                image_render_size, image_render_size, !disable_border, context.Theme.Context.Radius, !waiting
            );
                
            int fl_width = 0, fl_height = 0, sl_width = 0, sl_height = 0;
            Cairo.Color text_color = context.Theme.Colors.GetWidgetColor (GtkColorClass.Text, state);
            text_color.A = 0.75;
            
            Pango.Layout layout = context.Layout;
            layout.Width = (int)((cellWidth - cellHeight - x - 10) * Pango.Scale.PangoScale);
            layout.Ellipsize = Pango.EllipsizeMode.End;
            layout.FontDescription.Weight = Pango.Weight.Bold;
            
            // Compute the layout sizes for both lines for centering on the cell
            int old_size = layout.FontDescription.Size;
            
            layout.SetText (channel.Name ?? String.Empty);
            layout.GetPixelSize (out fl_width, out fl_height);
            
            if (channel.DbId > 0) {
                layout.FontDescription.Weight = Pango.Weight.Normal;
                layout.FontDescription.Size = (int)(old_size * Pango.Scale.Small);
                layout.FontDescription.Style = Pango.Style.Italic;
                
                if (channel.LastDownloadTime == DateTime.MinValue) {
                    layout.SetText (Catalog.GetString ("New!"));
                } else if (channel.LastDownloadTime.Date == DateTime.Now.Date) {
                    layout.SetText (String.Format (Catalog.GetString ("Last updated at {0}"), channel.LastDownloadTime.ToShortTimeString ()));
                } else {
                    layout.SetText (String.Format (Catalog.GetString ("Last updated on {0}"), channel.LastDownloadTime.ToLongDateString ()));
                }

                layout.GetPixelSize (out sl_width, out sl_height);
            }
            
            // Calculate the layout positioning
            x = ((int)cellHeight - x) + 10;
            y = (int)((cellHeight - (fl_height + sl_height)) / 2);
            
            // Render the second line first since we have that state already
            if (channel.DbId > 0) {
                context.Context.MoveTo (x, y + fl_height);
                context.Context.Color = text_color;
                PangoCairoHelper.ShowLayout (context.Context, layout);
            }
            
            // Render the first line, resetting the state
            //layout.SetText (channel.Name ?? String.Empty);
            layout.FontDescription.Weight = Pango.Weight.Bold;
            layout.FontDescription.Size = old_size;
            layout.FontDescription.Style = Pango.Style.Normal;
            
            layout.SetText (channel.Name ?? String.Empty);
            
            context.Context.MoveTo (x, y);
            text_color.A = 1;
            context.Context.Color = text_color;
            PangoCairoHelper.ShowLayout (context.Context, layout);
        }
        
        public int ComputeRowHeight (Widget widget)
        {
            int height;
            int text_w, text_h;
            
            Pango.Layout layout = new Pango.Layout (widget.PangoContext);
            layout.FontDescription = widget.PangoContext.FontDescription.Copy ();
            
            layout.FontDescription.Weight = Pango.Weight.Bold;
            layout.SetText ("W");
            layout.GetPixelSize (out text_w, out text_h);
            height = text_h;
            
            layout.FontDescription.Weight = Pango.Weight.Normal;
            layout.FontDescription.Size = (int)(layout.FontDescription.Size * Pango.Scale.Small);
            layout.FontDescription.Style = Pango.Style.Italic;
            layout.SetText ("W");
            layout.GetPixelSize (out text_w, out text_h);
            height += text_h;
            
            layout.Dispose ();
            
            return (height < image_size ? image_size : height) + 6;
        }
    }
}
