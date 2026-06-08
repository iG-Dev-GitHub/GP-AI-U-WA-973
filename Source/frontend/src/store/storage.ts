import AsyncStorage from '@react-native-async-storage/async-storage';

export type BlockType = 'perfect' | 'standard' | 'calm' | 'focus' | 'crooked';

export interface BlockRecord {
  id: string;
  type: BlockType;
  cycleNum: number;
  techniqueId: string;
}

export interface SessionRecord {
  id: string;
  date: string;
  techniqueId: string;
  techniqueName: string;
  cyclesCompleted: number;
  perfectBlocks: number;
  calmBlocks: number;
  focusFloors: number;
  totalFloors: number;
  badges: string[];
  durationSeconds: number;
  blocks: BlockRecord[];
}

export interface CustomRhythm {
  id: string;
  name: string;
  inhale: number;
  hold: number;
  exhale: number;
  hold2: number;
}

export interface AppSettings {
  soundEnabled: boolean;
  vibrationEnabled: boolean;
  customRhythms: CustomRhythm[];
}

export interface AppData {
  totalFloors: number;
  sessions: SessionRecord[];
  hasSeenTutorial: boolean;
  settings: AppSettings;
}

const STORAGE_KEY = 'breathTowerData_v1';

const defaultData: AppData = {
  totalFloors: 0,
  sessions: [],
  hasSeenTutorial: false,
  settings: {
    soundEnabled: true,
    vibrationEnabled: true,
    customRhythms: [],
  },
};

export async function loadAppData(): Promise<AppData> {
  try {
    const raw = await AsyncStorage.getItem(STORAGE_KEY);
    if (!raw) return { ...defaultData };
    const parsed = JSON.parse(raw);
    return { ...defaultData, ...parsed, settings: { ...defaultData.settings, ...(parsed.settings || {}) } };
  } catch {
    return { ...defaultData };
  }
}

export async function saveAppData(data: AppData): Promise<void> {
  await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(data));
}

export async function markTutorialSeen(): Promise<void> {
  const data = await loadAppData();
  data.hasSeenTutorial = true;
  await saveAppData(data);
}

export async function saveSession(session: SessionRecord): Promise<void> {
  const data = await loadAppData();
  data.sessions.unshift(session);
  data.totalFloors += session.totalFloors;
  await saveAppData(data);
}

export async function getSessionById(id: string): Promise<SessionRecord | null> {
  const data = await loadAppData();
  return data.sessions.find((s) => s.id === id) || null;
}

export async function updateSettings(settings: Partial<AppSettings>): Promise<void> {
  const data = await loadAppData();
  data.settings = { ...data.settings, ...settings };
  await saveAppData(data);
}

export async function resetAllData(): Promise<void> {
  await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(defaultData));
}

export async function saveCustomRhythm(rhythm: CustomRhythm): Promise<void> {
  const data = await loadAppData();
  data.settings.customRhythms.push(rhythm);
  await saveAppData(data);
}

export async function deleteCustomRhythm(id: string): Promise<void> {
  const data = await loadAppData();
  data.settings.customRhythms = data.settings.customRhythms.filter((r) => r.id !== id);
  await saveAppData(data);
}

export function generateSessionId(): string {
  return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
}

export function computeBadges(session: Omit<SessionRecord, 'badges'>): string[] {
  const badges: string[] = [];
  if (session.cyclesCompleted >= 10) badges.push('Peak Breath');
  if (session.focusFloors >= 1) badges.push('Focus Master');
  if (session.calmBlocks >= 3) badges.push('Calm Spirit');
  if (session.cyclesCompleted >= 20) badges.push('Tower Builder');
  if (session.cyclesCompleted >= 30) badges.push('Sky Raiser');
  return badges;
}
