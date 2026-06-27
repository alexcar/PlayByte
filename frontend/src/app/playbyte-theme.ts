import { definePreset } from '@primeng/themes';
import Aura from '@primeng/themes/aura';

/**
 * Preset de tema do PlayByte.
 * Deriva do Aura e sobrescreve a paleta primária (verde streaming #1DB954)
 * e as superfícies escuras para combinar com a identidade visual.
 */
export const PlayBytePreset = definePreset(Aura, {
  semantic: {
    primary: {
      50:  '#e9faf0',
      100: '#c9f2d8',
      200: '#97e6b4',
      300: '#5fd68c',
      400: '#33c46c',
      500: '#1db954', // verde streaming — cor principal
      600: '#19a449',
      700: '#14883c',
      800: '#106c30',
      900: '#0c5325',
      950: '#073016',
    },
    colorScheme: {
      dark: {
        surface: {
          0:   '#ffffff',
          50:  '#f6f8fa',
          100: '#e8f0f8',
          200: '#8899aa',
          300: '#5d7185',
          400: '#3a4d61',
          500: '#1e2f40', // borda
          600: '#16212e', // superfície (cards)
          700: '#13202c',
          800: '#0f1923', // fundo principal
          900: '#0b131b',
          950: '#070d13',
        },
        primary: {
          color: '#1db954',
          contrastColor: '#0f1923',
          hoverColor: '#33c46c',
          activeColor: '#19a449',
        },
        highlight: {
          background: 'rgba(29,185,84,0.12)',
          focusBackground: 'rgba(29,185,84,0.2)',
          color: '#1db954',
          focusColor: '#33c46c',
        },
      },
    },
  },
});
