import React, { useCallback, useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity,
  ScrollView, RefreshControl, Dimensions,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter, useFocusEffect } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { COLORS, SPACING, FONT_SIZE, RADIUS } from '../src/constants/theme';
import { loadAppData, SessionRecord } from '../src/store/storage';
import { getTechniqueColor } from '../src/constants/theme';

const { width } = Dimensions.get('window');

// City buildings for background
const BUILDINGS = [
  { x: 0, w: 50, h: 90 }, { x: 55, w: 35, h: 120 },
  { x: 93, w: 60, h: 72 }, { x: 156, w: 40, h: 100 },
  { x: 200, w: 55, h: 65 }, { x: 258, w: 38, h: 105 },
  { x: 298, w: 52, h: 82 }, { x: 352, w: 45, h: 115 },
  { x: 400, w: 50, h: 78 },
];

const WINDOWS = [
  { b: 0, x: 12, y: 20 }, { b: 0, x: 30, y: 20 }, { b: 0, x: 12, y: 45 }, { b: 0, x: 30, y: 45 }, { b: 0, x: 12, y: 65 },
  { b: 1, x: 8, y: 20 }, { b: 1, x: 20, y: 20 }, { b: 1, x: 8, y: 45 }, { b: 1, x: 20, y: 45 }, { b: 1, x: 8, y: 70 }, { b: 1, x: 20, y: 70 },
  { b: 2, x: 12, y: 18 }, { b: 2, x: 35, y: 18 }, { b: 2, x: 12, y: 40 }, { b: 2, x: 35, y: 40 },
  { b: 3, x: 10, y: 22 }, { b: 3, x: 25, y: 22 }, { b: 3, x: 10, y: 48 }, { b: 3, x: 25, y: 48 },
  { b: 4, x: 12, y: 15 }, { b: 4, x: 30, y: 15 }, { b: 4, x: 12, y: 38 },
  { b: 5, x: 10, y: 20 }, { b: 5, x: 22, y: 20 }, { b: 5, x: 10, y: 45 }, { b: 5, x: 22, y: 45 },
  { b: 6, x: 10, y: 18 }, { b: 6, x: 28, y: 18 }, { b: 6, x: 10, y: 40 }, { b: 6, x: 28, y: 40 },
  { b: 7, x: 12, y: 22 }, { b: 7, x: 28, y: 22 }, { b: 7, x: 12, y: 50 }, { b: 7, x: 28, y: 50 }, { b: 7, x: 12, y: 78 }, { b: 7, x: 28, y: 78 },
  { b: 8, x: 12, y: 20 }, { b: 8, x: 28, y: 20 }, { b: 8, x: 12, y: 48 },
];

function formatDuration(secs: number): string {
  const m = Math.floor(secs / 60);
  const s = secs % 60;
  return `${m}:${s.toString().padStart(2, '0')}`;
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

export default function Home() {
  const router = useRouter();
  const [totalFloors, setTotalFloors] = useState(0);
  const [sessions, setSessions] = useState<SessionRecord[]>([]);
  const [refreshing, setRefreshing] = useState(false);

  const loadData = useCallback(async () => {
    const data = await loadAppData();
    setTotalFloors(data.totalFloors);
    setSessions(data.sessions.slice(0, 20));
  }, []);

  useFocusEffect(useCallback(() => { loadData(); }, [loadData]));

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await loadData();
    setRefreshing(false);
  }, [loadData]);

  return (
    <SafeAreaView style={s.safe} edges={['top', 'bottom']}>
      <LinearGradient colors={['#0D1B2A', '#1B263B']} style={s.bg}>
        <ScrollView
          contentContainerStyle={s.scroll}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={COLORS.cyan} />}
          showsVerticalScrollIndicator={false}
        >
          {/* Header */}
          <View style={s.header}>
            <Text style={s.headerTitle}>Breath Tower</Text>
            <TouchableOpacity
              testID="settings-button"
              style={s.iconBtn}
              onPress={() => router.push('/settings')}
            >
              <Ionicons name="settings-outline" size={22} color={COLORS.textSecondary} />
            </TouchableOpacity>
          </View>

          {/* Tower Hero */}
          <View style={s.heroCard}>
            <LinearGradient colors={['#1B263B', '#162032']} style={s.heroGrad}>
              {/* Mini city for hero */}
              <View style={s.cityWrapper}>
                {BUILDINGS.map((b, i) => (
                  <View key={i} style={[s.building, { left: b.x, width: b.w, height: b.h }]}>
                    {WINDOWS.filter((w) => w.b === i).map((w, j) => (
                      <View key={j} style={[s.window, { left: w.x, bottom: w.y }]} />
                    ))}
                  </View>
                ))}
              </View>

              {/* Mini tower preview */}
              <View style={s.miniTower} testID="tower-preview">
                {Array.from({ length: Math.min(totalFloors, 14) }, (_, i) => (
                  <View
                    key={i}
                    style={[
                      s.miniBlock,
                      {
                        backgroundColor:
                          (i + 1) % 10 === 0 ? COLORS.blockFocus :
                          (i + 1) % 5 === 0 ? COLORS.blockCalm :
                          COLORS.blockPerfect,
                        shadowColor:
                          (i + 1) % 5 === 0 ? COLORS.blockCalm : COLORS.blockPerfect,
                        shadowOpacity: 0.5,
                        shadowRadius: 4,
                        elevation: 3,
                      },
                    ]}
                  />
                ))}
              </View>

              <View style={s.heroStats}>
                <Text style={s.heroFloors} testID="total-floors">{totalFloors}</Text>
                <Text style={s.heroLabel}>Total Floors</Text>
              </View>
            </LinearGradient>
          </View>

          {/* Actions */}
          <TouchableOpacity
            testID="new-session-button"
            style={s.mainBtn}
            onPress={() => router.push('/technique')}
            activeOpacity={0.85}
          >
            <LinearGradient
              colors={['#00CED1', '#4A9EFF']}
              style={s.mainBtnGrad}
              start={{ x: 0, y: 0 }}
              end={{ x: 1, y: 0 }}
            >
              <Ionicons name="add-circle-outline" size={22} color="#fff" />
              <Text style={s.mainBtnText}>New Session</Text>
            </LinearGradient>
          </TouchableOpacity>

          <TouchableOpacity
            testID="view-tower-button"
            style={s.secondBtn}
            onPress={() => router.push('/tower')}
            activeOpacity={0.85}
          >
            <Ionicons name="layers-outline" size={18} color={COLORS.textSecondary} />
            <Text style={s.secondBtnText}>View Full Tower</Text>
          </TouchableOpacity>

          {/* Sessions history */}
          <Text style={s.sectionTitle}>Recent Sessions</Text>

          {sessions.length === 0 ? (
            <View style={s.emptyCard}>
              <Text style={s.emptyEmoji}>🏗️</Text>
              <Text style={s.emptyText}>No sessions yet</Text>
              <Text style={s.emptySubtext}>Start your first breathing session to build your tower!</Text>
            </View>
          ) : (
            sessions.map((session) => (
              <View key={session.id} style={s.sessionCard} testID={`session-card-${session.id}`}>
                <View style={[s.techniqueTag, { backgroundColor: getTechniqueColor(session.techniqueId) + '22' }]}>
                  <View style={[s.techniqueTagDot, { backgroundColor: getTechniqueColor(session.techniqueId) }]} />
                  <Text style={[s.techniqueTagText, { color: getTechniqueColor(session.techniqueId) }]}>
                    {session.techniqueName}
                  </Text>
                </View>

                <View style={s.sessionRow}>
                  <View style={s.sessionStat}>
                    <Text style={s.sessionStatVal}>{session.totalFloors}</Text>
                    <Text style={s.sessionStatLabel}>Floors</Text>
                  </View>
                  <View style={s.sessionDivider} />
                  <View style={s.sessionStat}>
                    <Text style={s.sessionStatVal}>{session.perfectBlocks}</Text>
                    <Text style={s.sessionStatLabel}>Perfect</Text>
                  </View>
                  <View style={s.sessionDivider} />
                  <View style={s.sessionStat}>
                    <Text style={s.sessionStatVal}>{formatDuration(session.durationSeconds)}</Text>
                    <Text style={s.sessionStatLabel}>Duration</Text>
                  </View>
                  <View style={s.sessionDateWrapper}>
                    <Text style={s.sessionDate}>{formatDate(session.date)}</Text>
                    {session.badges.length > 0 && (
                      <View style={s.badgeRow}>
                        {session.badges.slice(0, 2).map((badge) => (
                          <View key={badge} style={s.badge}>
                            <Text style={s.badgeText}>{badge}</Text>
                          </View>
                        ))}
                      </View>
                    )}
                  </View>
                </View>
              </View>
            ))
          )}
          <View style={{ height: SPACING.xl }} />
        </ScrollView>
      </LinearGradient>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  bg: { flex: 1 },
  scroll: { paddingHorizontal: SPACING.md, paddingTop: SPACING.sm },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: SPACING.lg, paddingHorizontal: SPACING.xs },
  headerTitle: { color: COLORS.textPrimary, fontSize: FONT_SIZE.xl, fontWeight: '700' },
  iconBtn: { padding: SPACING.sm, borderRadius: RADIUS.pill, backgroundColor: COLORS.surface },
  heroCard: { borderRadius: RADIUS.xl, overflow: 'hidden', marginBottom: SPACING.md },
  heroGrad: { padding: SPACING.md, minHeight: 240, justifyContent: 'flex-end', overflow: 'hidden' },
  cityWrapper: { position: 'absolute', bottom: 0, left: 0, right: 0, height: 130 },
  building: { position: 'absolute', bottom: 0, backgroundColor: '#0D1B2A' },
  window: { position: 'absolute', width: 6, height: 5, backgroundColor: '#FFF8C0', borderRadius: 1, opacity: 0.7 },
  miniTower: { alignSelf: 'center', alignItems: 'center', gap: 2, marginBottom: SPACING.sm },
  miniBlock: { width: 80, height: 14, borderRadius: 4 },
  heroStats: { alignItems: 'center', paddingBottom: SPACING.sm },
  heroFloors: { color: COLORS.textPrimary, fontSize: FONT_SIZE.huge, fontWeight: '800' },
  heroLabel: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, letterSpacing: 1.5, textTransform: 'uppercase' },
  mainBtn: { borderRadius: RADIUS.pill, overflow: 'hidden', marginBottom: SPACING.sm },
  mainBtnGrad: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: SPACING.sm, paddingVertical: SPACING.md },
  mainBtnText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '700' },
  secondBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: SPACING.sm, paddingVertical: SPACING.md, marginBottom: SPACING.lg },
  secondBtnText: { color: COLORS.textSecondary, fontSize: FONT_SIZE.md, fontWeight: '600' },
  sectionTitle: { color: COLORS.textSecondary, fontSize: FONT_SIZE.xs, fontWeight: '700', letterSpacing: 1.5, textTransform: 'uppercase', marginBottom: SPACING.sm },
  emptyCard: { alignItems: 'center', padding: SPACING.xl, backgroundColor: COLORS.surface, borderRadius: RADIUS.xl },
  emptyEmoji: { fontSize: 48, marginBottom: SPACING.md },
  emptyText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '600', marginBottom: SPACING.xs },
  emptySubtext: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, textAlign: 'center' },
  sessionCard: { backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, padding: SPACING.md, marginBottom: SPACING.sm },
  techniqueTag: { flexDirection: 'row', alignItems: 'center', gap: SPACING.xs, alignSelf: 'flex-start', paddingHorizontal: SPACING.sm, paddingVertical: 4, borderRadius: RADIUS.pill, marginBottom: SPACING.sm },
  techniqueTagDot: { width: 6, height: 6, borderRadius: 3 },
  techniqueTagText: { fontSize: FONT_SIZE.xs, fontWeight: '600' },
  sessionRow: { flexDirection: 'row', alignItems: 'center' },
  sessionStat: { alignItems: 'center', flex: 1 },
  sessionStatVal: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '700' },
  sessionStatLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, marginTop: 2 },
  sessionDivider: { width: 1, height: 30, backgroundColor: 'rgba(255,255,255,0.1)' },
  sessionDateWrapper: { flex: 1.5, alignItems: 'flex-end' },
  sessionDate: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs },
  badgeRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 4, marginTop: 4, justifyContent: 'flex-end' },
  badge: { backgroundColor: 'rgba(255,215,0,0.15)', paddingHorizontal: 6, paddingVertical: 2, borderRadius: RADIUS.pill },
  badgeText: { color: COLORS.gold, fontSize: 9, fontWeight: '700' },
});
