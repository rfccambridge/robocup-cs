using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Geometry;
using System.Drawing;

namespace RFC.Messaging
{
    public class VisualDebugMessage : Message
    {
        public Point2 position;
        public Color c;
        public bool clear;

        // this will add a marker
        public VisualDebugMessage(Point2 pos, Color c)
        {
            this.c = c;
            this.position = pos;
            this.clear = false;
        }

        // this will clear all markers
        public VisualDebugMessage()
        {
            this.clear = true;
        }
    }


    public class VisualDebugMessage<T> : Message
    {
        public T value;

        public VisualDebugMessage(T value)
        {
            this.value = value;
        }
    }
}
