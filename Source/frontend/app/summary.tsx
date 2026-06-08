import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, ScrollView,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter, useLocalSearchParams } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import Animated, { useSharedValue, useAnimatedStyle, withSpring, withDelay } from 'react-native-reanimated';
import { COLORS, SPACING, FONT_SIZE, RADIUS, getTechniqueColor } from '../src/constants/theme';
import { getSessionById, SessionRecord } from '../src/store/storage';

function StatBox({ value, label, color }: { value: string | number; label: string; color?: string }) {
  const scale = useSharedValue(0);
  useEffect(() => { scale.value = withSpring(1, { damping: 14, stiffness: 80 }); }, []);
  const st = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));
  return (
    <Animated.View style={[s.statBox, st]}>
      <Text style={[s.statVal, { color: color ?? COLORS.textPrimary }]}>{value}</Text>
      <Text style={s.statLabel}>{label}</Text>
    </Animated.View>
  );
}

const BADGE_ICONS: Record<string, string> = {
  'Peak Breath': '🏔️',
  'Focus Master': '🎯',
  'Calm Spirit': '🌊',
  'Tower Builder': '🏗️',
  'Sky Raiser': '🌟',
};

function formatDuration(secs: number): string {
  const m = Math.floor(secs / 60);
  const s = secs % 60;
  if (m === 0) return `${s}s`;
  return `${m}m ${s}s`;
}

export default function Summary() {
  const router = useRouter();
  const { sessionId } = useLocalSearchParams<{ sessionId: string }>();
  const [session, setSession] = useState<SessionRecord | null>(null);
  const [loading, setLoading] = useState(true);

  const headerSlide = useSharedValue(-30);
  const headerOp = useSharedValue(0);

  useEffect(() => {
    if (!sessionId) return;
    getSessionById(sessionId).then((s) => {
      setSession(s);
      setLoading(false);
      headerSlide.value = withSpring(0, { damping: 16 });
      headerOp.value = withDelay(100, withSpring(1));
    });
  }, [sessionId]);

  const headerStyle = useAnimatedStyle(() => ({
    transform: [{ translateY: headerSlide.value }],
    opacity: headerOp.value,
  }));

  if (loading || !session) {
    return (
      <SafeAreaView style={s.safe} edges={['top', 'bottom']}>
        <LinearGradient colors={['#0D1B2A', '#1B263B']} style={s.bg} />
      </SafeAreaView>
    );
  }

  const techniqueColor = getTechniqueColor(session.techniqueId);

  return (
    <SafeAreaView style={s.safe} edges={['top', 'bottom']}>
      <LinearGradient colors={['#0D1B2A', '#1B263B']} style={s.bg}>
        <ScrollView contentContainerStyle={s.scroll} showsVerticalScrollIndicator={false}>
          {/* Header */}
          <Animated.View style={[s.heroSection, headerStyle]}>
            <Text style={s.heroEmoji}>
              {session.cyclesCompleted >= 10 ? '🌟' : session.cyclesCompleted >= 5 ? '✨' : '🧘'}
            </Text>
            <Text style={s.heroTitle}>Session Complete!</Text>
            <View style={[s.techTag, { backgroundColor: techniqueColor + '22' }]}>
              <View style={[s.techDot, { backgroundColor: techniqueColor }]} />
              <Text style={[s.techTagText, { color: techniqueColor }]}>{session.techniqueName}</Text>
            </View>
          </Animated.View>

          {/* Main stats */}
          <View style={s.statsGrid} testID="session-stats">
            <StatBox value={session.totalFloors} label="Floors Built" color={COLORS.textPrimary} />
            <StatBox value={session.cyclesCompleted} label="Cycles" color={COLORS.cyan} />
            <StatBox value={session.perfectBlocks} label="Perfect ✨" color={COLORS.gold} />
            <StatBox value={formatDuration(session.durationSeconds)} label="Duration" color={COLORS.textSecondary} />
          </View>

          {/* Block types */}
          {(session.calmBlocks > 0 || session.focusFloors > 0) && (
            <View style={s.bonusCard}>
              <Text style={s.bonusTitle}>Special Blocks</Text>
              <View style={s.bonusRow}>
                {session.calmBlocks > 0 && (
                  <View style={s.bonusItem}>
                    <View style={[s.bonusDot, { backgroundColor: COLORS.blockCalm }]} />
                    <Text style={s.bonusText}>{session.calmBlocks} Calm Block{session.calmBlocks > 1 ? 's' : ''}</Text>
                  </View>
                )}
                {session.focusFloors > 0 && (
                  <View style={s.bonusItem}>
                    <Text style={s.bonusIcon}>★</Text>
                    <Text style={s.bonusText}>{session.focusFloors} Focus Floor{session.focusFloors > 1 ? 's' : ''}</Text>
                  </View>
                )}
              </View>
            </View>
          )}

          {/* Tower mini preview */}
          <View style={s.towerCard}>
            <Text style={s.towerTitle}>This Session's Tower</Text>
            <View style={s.towerPreview} testID="tower-mini-preview">
              {session.blocks.slice(-12).reverse().map((block, i) => (
                <View
                  key={block.id}
                  style={[
                    s.previewBlock,
                    {
                      backgroundColor: block.type === 'focus' || block.type === 'perfect' ? COLORS.blockPerfect
                        : block.type === 'calm' ? COLORS.blockCalm
                        : getTechniqueColor(session.techniqueId),
                    },
                  ]}
                />
              ))}
            </View>
          </View>

          {/* Badges */}
          {session.badges.length > 0 && (
            <View style={s.badgesCard}>
              <Text style={s.badgesTitle}>Badges Earned 🏅</Text>
              <View style={s.badgesRow}>
                {session.badges.map((badge) => (
                  <View key={badge} style={s.badge} testID={`badge-${badge.replace(/\s+/g, '-').toLowerCase()}`}>
                    <Text style={s.badgeEmoji}>{BADGE_ICONS[badge] ?? '🏅'}</Text>
                    <Text style={s.badgeText}>{badge}</Text>
                  </View>
                ))}
              </View>
            </View>
          )}

          {/* Actions */}
          <TouchableOpacity
            testID="build-again-button"
            style={s.mainBtn}
            onPress={() => router.replace('/technique')}
            activeOpacity={0.85}
          >
            <LinearGradient
              colors={['#00CED1', '#4A9EFF']}
              style={s.mainBtnGrad}
              start={{ x: 0, y: 0 }}
              end={{ x: 1, y: 0 }}
            >
              <Ionicons name="refresh" size={18} color="#fff" />
              <Text style={s.mainBtnText}>Build Again</Text>
            </LinearGradient>
          </TouchableOpacity>

          <TouchableOpacity
            testID="go-home-button"
            style={s.homeBtn}
            onPress={() => router.replace('/home')}
            activeOpacity={0.8}
          >
            <Ionicons name="home-outline" size={18} color={COLORS.textSecondary} />
            <Text style={s.homeBtnText}>Go Home</Text>
          </TouchableOpacity>

          <View style={{ height: SPACING.xl }} />
        </ScrollView>
      </LinearGradient>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  bg: { flex: 1 },
  scroll: { paddingHorizontal: SPACING.md, paddingTop: SPACING.lg },
  heroSection: { alignItems: 'center', marginBottom: SPACING.xl },
  heroEmoji: { fontSize: 64, marginBottom: SPACING.sm },
  heroTitle: { color: COLORS.textPrimary, fontSize: FONT_SIZE.xxl, fontWeight: '800', marginBottom: SPACING.sm },
  techTag: { flexDirection: 'row', alignItems: 'center', gap: SPACING.xs, paddingHorizontal: SPACING.md, paddingVertical: SPACING.xs, borderRadius: RADIUS.pill },
  techDot: { width: 8, height: 8, borderRadius: 4 },
  techTagText: { fontSize: FONT_SIZE.sm, fontWeight: '600' },
  statsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: SPACING.sm, marginBottom: SPACING.md },
  statBox: { flex: 1, minWidth: '45%', backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, padding: SPACING.md, alignItems: 'center' },
  statVal: { fontSize: FONT_SIZE.xxxl, fontWeight: '800' },
  statLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, marginTop: 4, textTransform: 'uppercase', letterSpacing: 1 },
  bonusCard: { backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, padding: SPACING.md, marginBottom: SPACING.md },
  bonusTitle: { color: COLORS.textSecondary, fontSize: FONT_SIZE.xs, fontWeight: '700', letterSpacing: 1.5, textTransform: 'uppercase', marginBottom: SPACING.sm },
  bonusRow: { flexDirection: 'row', gap: SPACING.lg },
  bonusItem: { flexDirection: 'row', alignItems: 'center', gap: SPACING.xs },
  bonusDot: { width: 10, height: 10, borderRadius: 5 },
  bonusIcon: { color: COLORS.gold, fontSize: 14 },
  bonusText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  towerCard: { backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, padding: SPACING.md, marginBottom: SPACING.md },
  towerTitle: { color: COLORS.textSecondary, fontSize: FONT_SIZE.xs, fontWeight: '700', letterSpacing: 1.5, textTransform: 'uppercase', marginBottom: SPACING.md },
  towerPreview: { alignItems: 'center', gap: 3 },
  previewBlock: { width: 100, height: 14, borderRadius: 4 },
  badgesCard: { backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, padding: SPACING.md, marginBottom: SPACING.lg },
  badgesTitle: { color: COLORS.textSecondary, fontSize: FONT_SIZE.xs, fontWeight: '700', letterSpacing: 1.5, textTransform: 'uppercase', marginBottom: SPACING.md },
  badgesRow: { flexDirection: 'row', flexWrap: 'wrap', gap: SPACING.sm },
  badge: { alignItems: 'center', padding: SPACING.md, backgroundColor: 'rgba(255,215,0,0.1)', borderRadius: RADIUS.lg, minWidth: 90, borderWidth: 1, borderColor: 'rgba(255,215,0,0.3)' },
  badgeEmoji: { fontSize: 28, marginBottom: SPACING.xs },
  badgeText: { color: COLORS.gold, fontSize: FONT_SIZE.xs, fontWeight: '700', textAlign: 'center' },
  mainBtn: { borderRadius: RADIUS.pill, overflow: 'hidden', marginBottom: SPACING.sm },
  mainBtnGrad: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: SPACING.sm, paddingVertical: SPACING.md },
  mainBtnText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '700' },
  homeBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: SPACING.sm, paddingVertical: SPACING.md },
  homeBtnText: { color: COLORS.textSecondary, fontSize: FONT_SIZE.md, fontWeight: '600' },
});
