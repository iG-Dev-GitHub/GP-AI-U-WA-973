export type PhaseName = 'inhale' | 'hold' | 'exhale' | 'hold2';

export interface Phase {
  name: PhaseName;
  duration: number; // seconds
  label: string;
  colorKey: 'inhale' | 'hold' | 'exhale';
}

export interface Technique {
  id: string;
  name: string;
  description: string;
  rhythm: string; // e.g. "4-7-8" or "4-4-4-4"
  phases: Phase[];
  colorId: string;
  totalCycleDuration: number;
}

export const TECHNIQUES: Technique[] = [
  {
    id: '4-7-8',
    name: '4-7-8 Breathing',
    description: 'Promotes deep relaxation and helps reduce anxiety. Inhale for 4s, hold for 7s, exhale for 8s.',
    rhythm: '4 — 7 — 8',
    phases: [
      { name: 'inhale', duration: 4, label: 'Inhale', colorKey: 'inhale' },
      { name: 'hold', duration: 7, label: 'Hold', colorKey: 'hold' },
      { name: 'exhale', duration: 8, label: 'Exhale', colorKey: 'exhale' },
    ],
    colorId: '4-7-8',
    totalCycleDuration: 19,
  },
  {
    id: 'box',
    name: 'Box Breathing',
    description: 'Used by Navy SEALs to manage stress. Equal duration for all 4 phases.',
    rhythm: '4 — 4 — 4 — 4',
    phases: [
      { name: 'inhale', duration: 4, label: 'Inhale', colorKey: 'inhale' },
      { name: 'hold', duration: 4, label: 'Hold', colorKey: 'hold' },
      { name: 'exhale', duration: 4, label: 'Exhale', colorKey: 'exhale' },
      { name: 'hold2', duration: 4, label: 'Hold', colorKey: 'hold' },
    ],
    colorId: 'box',
    totalCycleDuration: 16,
  },
  {
    id: 'even',
    name: 'Equal Breathing',
    description: 'Simple and balanced. Equal inhale and exhale creates calm focus.',
    rhythm: '4 — 4',
    phases: [
      { name: 'inhale', duration: 4, label: 'Inhale', colorKey: 'inhale' },
      { name: 'exhale', duration: 4, label: 'Exhale', colorKey: 'exhale' },
    ],
    colorId: 'even',
    totalCycleDuration: 8,
  },
  {
    id: 'custom',
    name: 'Custom Rhythm',
    description: 'Define your own breathing pattern for a personalized practice.',
    rhythm: 'Custom',
    phases: [
      { name: 'inhale', duration: 4, label: 'Inhale', colorKey: 'inhale' },
      { name: 'hold', duration: 2, label: 'Hold', colorKey: 'hold' },
      { name: 'exhale', duration: 4, label: 'Exhale', colorKey: 'exhale' },
    ],
    colorId: 'custom',
    totalCycleDuration: 10,
  },
];

export const buildCustomTechnique = (
  inhale: number,
  hold: number,
  exhale: number,
  hold2: number
): Technique => {
  const phases: Phase[] = [
    { name: 'inhale', duration: inhale, label: 'Inhale', colorKey: 'inhale' },
  ];
  if (hold > 0) phases.push({ name: 'hold', duration: hold, label: 'Hold', colorKey: 'hold' });
  phases.push({ name: 'exhale', duration: exhale, label: 'Exhale', colorKey: 'exhale' });
  if (hold2 > 0) phases.push({ name: 'hold2', duration: hold2, label: 'Hold', colorKey: 'hold' });

  return {
    id: 'custom',
    name: 'Custom Rhythm',
    description: `Inhale ${inhale}s${hold > 0 ? ` · Hold ${hold}s` : ''} · Exhale ${exhale}s${hold2 > 0 ? ` · Hold ${hold2}s` : ''}`,
    rhythm: [inhale, hold > 0 ? hold : null, exhale, hold2 > 0 ? hold2 : null].filter(Boolean).join(' — '),
    phases,
    colorId: 'custom',
    totalCycleDuration: inhale + hold + exhale + hold2,
  };
};

export type BlockType = 'perfect' | 'standard' | 'calm' | 'focus';

export const determineBlockType = (cycleNum: number): BlockType => {
  if (cycleNum > 0 && cycleNum % 10 === 0) return 'focus';
  if (cycleNum > 0 && cycleNum % 5 === 0) return 'calm';
  return 'perfect';
};

export const getPhaseColor = (phaseName: PhaseName): string => {
  if (phaseName === 'inhale') return '#00CED1';
  if (phaseName === 'hold' || phaseName === 'hold2') return '#FFFFFF';
  return '#4A9EFF';
};
