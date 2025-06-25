#define HIGH_PRECISION_VERTEX

#include "sh_Utils.h"
#include "sh_Masking.h"

precision highp float;

layout(std140, set = 0, binding = 0) uniform m_GraphPartTriangleParameters
{
    highp float startAngle;
    highp float endAngle;
    highp float startPosition;
    highp float endPosition;
    mediump vec2 size;
    lowp vec4 backgroundColour;
};

layout(location = 2) in highp vec2 v_TexCoord;

layout(location = 0) out vec4 o_Colour;

vec4 Triangle(in highp float _angle, in highp float _radius)
{
    // Normalize the angle into [startAngle, endAngle]
    _angle = mod(_angle - startAngle, 2.0 * 3.14159265359) + startAngle;
    
    if (_angle < startAngle || _angle > endAngle)
        return vec4(0);

    highp float sinDelta = sin(endAngle - startAngle);
    highp float sinTheta2 = sin(endAngle - _angle);
    highp float sinTheta1 = sin(_angle - startAngle);
    
    highp float maxRadius = (startPosition * endPosition * sinDelta) / 
                          (endPosition * sinTheta2 + startPosition * sinTheta1);

    if (_radius > maxRadius)
        return vec4(0);

    return backgroundColour;
}

void main(void)
{
    // Get the position of every pixel relative to our drawing space.
    highp vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    highp vec2 pixelPos = (v_TexCoord - v_TexRect.xy) / resolution;

    // Turn the relative position into our familiar absolute position.
    mediump vec2 absolutePos = size * pixelPos;
    highp vec2 st = v_TexCoord / (v_TexRect.zw - v_TexRect.xy);

    // 计算当前点的极坐标 (X+为0度)
    highp float currentRadius = distance(pixelPos, vec2(0.5));
    highp float currentAngle = atan(pixelPos.y - 0.5, pixelPos.x - 0.5);
    
    // 处理原点附近点
    if (currentRadius < 0.001) {
        o_Colour = backgroundColour;
        return;
    }

    o_Colour = Triangle(currentAngle, currentRadius);
}
