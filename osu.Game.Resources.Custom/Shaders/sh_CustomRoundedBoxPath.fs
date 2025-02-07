#define HIGH_PRECISION_VERTEX

#include "sh_Utils.h"
#include "sh_Masking.h"

precision highp float;

layout(std140, set = 0, binding = 0) uniform m_CustomRoundedBoxPathParameters
{
    lowp vec4 backgroundColour;
    lowp vec4 borderColour;
    // Assuming we are receiving truly absolute position data... 
    mediump vec2 size;
    mediump float borderWidth;
};

layout(location = 2) in highp vec2 v_TexCoord;

layout(location = 0) out vec4 o_Colour;

// Centre as Anchor
vec4 Rect(in vec2 _st, in vec2 _origin, in vec2 _size)
{
    // Draw Origin Dot (Centre)
    // if (distance(_st, _origin) < 0.02) return vec4(0.0, 1.0, 0.0, 1.0);

    if (abs(_st.x - _origin.x) <= _size.x / 2.0 && abs(_st.y - _origin.y) <= _size.y / 2.0)
        return backgroundColour;
    
    return vec4(0.0);
}

vec4 AltCircle(in vec2 _st, in vec2 _origin, in highp float _radius)
{
    vec2 dist = _st - _origin;
    
    // Draw Origin Dot (Centre)
    if (distance(_st, _origin) <= _radius) return backgroundColour;
    return vec4(0.0);
}

vec4 CustomRect(in vec2 _abspos, in vec2 _origin, in vec2 _size, in highp float _border)
{
    vec2 leftP = vec2(_size.y / 2.0, _origin.y);
    vec2 rightP = vec2(_size.x - _size.y / 2.0, _origin.y);
    highp float circleRadius = _size.y / 2.0;
    vec2 mainRectSize = vec2(_size.x - _size.y, _size.y);
    
    // The main rectangle
    if (_abspos.x >= leftP.x && _abspos.x <= rightP.x && abs(_abspos.y - _origin.y) <= _size.y / 2.0)
        return backgroundColour; // Rect(_abspos, _origin, mainRectSize);
    
    // Top left corner square
    if (_abspos.x - leftP.x <= 0.0 && _abspos.x - leftP.x >= -(_size.y / 2.0) && _abspos.y - leftP.y <= 0.0 && _abspos.y - leftP.y >= -(_size.y / 2.0))
        return backgroundColour;
    
    /*
    if (abs(distance(_abspos, leftP) - _size.y / 2.0) <= 0.01 && leftP.x - _abspos.x >= 0.0)
        return borderColour;
    
    if (abs(distance(_abspos, leftP) - _size.y / 2.0) <= 0.01 && _abspos.x - rightP.x >= 0.0)
        return borderColour;
    */

    // Two Circles
    if (distance(_abspos, leftP) <= _size.y / 2.0 && leftP.x - _abspos.x >= 0.0)
        return AltCircle(_abspos, leftP, _size.y / 2.0);
    
    if (distance(_abspos, rightP) <= _size.y / 2.0 && _abspos.x - rightP.x >= 0.0)
        return AltCircle(_abspos, rightP, _size.y / 2.0);
    
    return vec4(0.0);
}

void main(void)
{
    // Get the position of every pixel relative to our drawing space.
    highp vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    highp vec2 pixelPos = (v_TexCoord - v_TexRect.xy) / resolution;

    // Turn the relative position into our familiar absolute position.
    mediump vec2 absolutePos = size * pixelPos;
    highp vec2 st = v_TexCoord / (v_TexRect.zw - v_TexRect.xy);

    o_Colour = vec4(CustomRect(absolutePos, size / 2.0, size, 1.0));
}
