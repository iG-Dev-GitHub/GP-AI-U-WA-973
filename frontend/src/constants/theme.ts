export const COLORS = {
  bg: '#0D1B2A',
  surface: '#1B263B',
  surfaceAlt: '#162032',
  surfaceHighlight: '#415A77',
  textPrimary: '#FFFFFF',
  textSecondary: 'rgba(255,255,255,0.7)',
  textMuted: 'rgba(255,255,255,0.4)',

  blockStandard: '#87CEEB',
  blockPerfect: '#FFD700',
  blockCalm: '#00CED1',
  blockFocus: '#FFD700',
  blockCrooked: '#FF6B6B',

  technique478: '#9D4CDD',
  techniqueBox: '#4A9EFF',
  techniqueEven: '#87CEEB',
  techniqueCustom: '#FFFFFF',

  inhale: '#00CED1',
  hold: '#FFFFFF',
  exhale: '#4A9EFF',

  crane: '#8FA3B1',
  gold: '#FFD700',
  cyan: '#00CED1',
  purple: '#9D4CDD',

  success: '#4CAF50',
  danger: '#FF6B6B',
  warning: '#FFA726',

  gradientTop: '#0D1B2A',
  gradientMid: '#1B263B',
};

export const SPACING = {
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  xxl: 48,
};

export const RADIUS = {
  sm: 8,
  md: 12,
  lg: 20,
  xl: 28,
  pill: 9999,
};

export const FONT_SIZE = {
  xs: 11,
  sm: 13,
  md: 15,
  lg: 18,
  xl: 22,
  xxl: 28,
  xxxl: 36,
  huge: 48,
};

export const getTechniqueColor = (id: string): string => {
  switch (id) {
    case '4-7-8': return COLORS.technique478;
    case 'box': return COLORS.techniqueBox;
    case 'even': return COLORS.techniqueEven;
    case 'custom': return COLORS.techniqueCustom;
    default: return COLORS.blockStandard;
  }
};
