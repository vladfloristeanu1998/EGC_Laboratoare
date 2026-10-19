#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;

out vec3 vColor;

uniform float uTime;
uniform bool uWave;
uniform int uColorMode;

void main()
{
    vec3 pos = aPosition;

    // P5a: Deformarea de tip unda
    if (uWave) {
        pos.y += 0.1 * sin(10.0 * pos.x + uTime);
    }

    gl_Position = vec4(pos, 1.0);

    // P5c: Culoare calculata din pozitie
    if (uColorMode == 1) {
        vColor = vec3(pos.xy * 0.5 + vec2(0.5), 0.0);
    } else {
        vColor = aColor;
    }
}