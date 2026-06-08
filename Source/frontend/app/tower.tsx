import React, { useCallback, useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, ScrollView, Dimensions,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter, useFocusEffect } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { COLORS, SPACING, FONT_SIZE, RADIUS, getTechniqueColor } from '../src/constants/theme';
import { loadAppData, SessionRecord, BlockRecord } from '../src/store/storage';

const { width } = Dimensions.get('window');
const BLOCK_W = width - SPACING.md * 4;

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function BlockColor(block: BlockRecord, techniqueId: string): string {
  if (block.type === 'focus' || block.type === 'perfect') return COLORS.blockPerfect;
  if (block.type === 'calm') return COLORS.blockCalm;
  return getTechniqueColor(techniqueId);
}

export default function TowerScreen() {
  const router = useRouter();
  const [totalFloors, setTotalFloors] = useState(0);
  const [sessions, setSessions] = useState<SessionRecord[]>([]);

  useFocusEffect(useCallback(() => {
    loadAppData().then((data) => {
      setTotalFloors(data.totalFloors);
      setSessions(data.sessions);
    });
  }, []));

  // All blocks across all sessions (newest first)
  const allBlocks: Array<{ block: BlockRecord; techniqueId: string; sessionDate: string }> = [];
  for (const session of sessions) {
    for (const block of session.blocks) {
      allBlocks.push({ block, techniqueId: session.techniqueId, sessionDate: session.date });
    }
  }
  allBlocks.reverse();

  return (
    <SafeAreaView style={s.safe} edges={['top', 'bottom']}>
      <LinearGradient colors={['#0D1B2A', '#1B263B']} style={s.bg}>
        {/* Header */}
        <View style={s.header}>
          <TouchableOpacity testID="back-button" style={s.backBtn} onPress={() => router.back()}>
            <Ionicons name="arrow-back" size={22} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={s.headerTitle}>Your Tower</Text>
          <View style={s.floorsBadge}>
            <Text style={s.floorsNum}>{totalFloors}</Text>
            <Text style={s.floorsUnit}> fl</Text>
          </View>
        </View>

        <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={s.scroll}>
          {/* Total floors hero */}
          <LinearGradient colors={['#1B263B', '#0D1B2A']} style={s.heroCard}>
            <Text style={s.heroNum} testID="tower-total-floors">{totalFloors}</Text>
            <Text style={s.heroLabel}>Total Floors</Text>
            {totalFloors > 0 && (
              <View style={s.heroSubRow}>
                <Text style={s.heroSub}>{sessions.length} session{sessions.length !== 1 ? 's' : ''}</Text>
                <View style={s.heroDot} />
                <Text style={s.heroSub}>{allBlocks.filter(({ block: b }) => b.type === 'focus').length} Focus Floors</Text>
              </View>
            )}
          </LinearGradient>

          {/* Tower visualization */}
          {allBlocks.length === 0 ? (
            <View style={s.emptyCard}>
              <Text style={s.emptyEmoji}>🏗️</Text>
              <Text style={s.emptyText}>Your tower is empty</Text>
              <Text style={s.emptySubtext}>Complete breathing sessions to build your tower floor by floor.</Text>
              <TouchableOpacity
                testID="start-first-session-button"
                style={s.startBtn}
                onPress={() => router.push('/technique')}
              >
                <Text style={s.startBtnText}>Start First Session</Text>
              </TouchableOpacity>
            </View>
          ) : (
            <>
              <Text style={s.sectionLabel}>TOWER BLOCKS (newest on top)</Text>
              <View style={s.towerContainer} testID="full-tower-view">
                {/* Sessions as sections */}
                {sessions.map((session, sIdx) => (
                  <View key={session.id}>
                    {/* Session divider */}
                    <View style={s.sessionDivider}>
                      <View style={[s.sessionDividerLine, { backgroundColor: getTechniqueColor(session.techniqueId) + '44' }]} />
                      <View style={[s.sessionDividerTag, { backgroundColor: getTechniqueColor(session.techniqueId) + '22' }]}>
                        <Text style={[s.sessionDividerText, { color: getTechniqueColor(session.techniqueId) }]}>
                          {formatDate(session.date)} · {session.totalFloors}f
                        </Text>
                      </View>
                      <View style={[s.sessionDividerLine, { backgroundColor: getTechniqueColor(session.techniqueId) + '44' }]} />
                    </View>

                    {/* Blocks for this session (newest first in the section) */}
                    {[...session.blocks].reverse().map((block) => (
                      <View
                        key={block.id}
                        testID={`tower-block-${block.id}`}
                        style={[
                          s.towerBlock,
                          { backgroundColor: BlockColor(block, session.techniqueId) },
                          block.type === 'focus' && s.focusBlock,
                          block.type === 'calm' && s.calmBlock,
                        ]}
                      >
                        {block.type === 'focus' && (
                          <View style={s.focusFloorLabel}>
                            <Text style={s.focusFloorText}>★ Focus Floor #{block.cycleNum}</Text>
                          </View>
                        )}
                        {block.type === 'calm' && (
                          <View style={s.calmStripe} />
                        )}
                      </View>
                    ))}
                  </View>
                ))}
              </View>
            </>
          )}

          {/* Session history */}
          {sessions.length > 0 && (
            <>
              <Text style={[s.sectionLabel, { marginTop: SPACING.xl }]}>SESSION HISTORY</Text>
              {sessions.map((session) => (
                <View key={session.id} style={s.sessionCard} testID={`session-history-${session.id}`}>
                  <View style={s.sessionCardRow}>
                    <View style={[s.sessionColorBar, { backgroundColor: getTechniqueColor(session.techniqueId) }]} />
                    <View style={s.sessionCardContent}>
                      <Text style={s.sessionCardDate}>{formatDate(session.date)}</Text>
                      <Text style={s.sessionCardTech}>{session.techniqueName}</Text>
                    </View>
                    <View style={s.sessionCardStats}>
                      <Text style={s.sessionCardFloors}>{session.totalFloors}</Text>
                      <Text style={s.sessionCardFloorsLabel}>floors</Text>
                    </View>
                  </View>
                  {session.badges.length > 0 && (
                    <View style={s.badgeRow}>
                      {session.badges.map((badge) => (
                        <View key={badge} style={s.badge}>
                          <Text style={s.badgeText}>{badge}</Text>
                        </View>
                      ))}
                    </View>
                  )}
                </View>
              ))}
            </>
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
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SPACING.md, paddingVertical: SPACING.sm },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.surface, alignItems: 'center', justifyContent: 'center' },
  headerTitle: { color: COLORS.textPrimary, fontSize: FONT_SIZE.xl, fontWeight: '700' },
  floorsBadge: { flexDirection: 'row', alignItems: 'baseline', backgroundColor: COLORS.surface, paddingHorizontal: SPACING.sm, paddingVertical: SPACING.xs, borderRadius: RADIUS.pill },
  floorsNum: { color: COLORS.gold, fontSize: FONT_SIZE.lg, fontWeight: '800' },
  floorsUnit: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs },
  scroll: { paddingHorizontal: SPACING.md, paddingTop: SPACING.xs },
  heroCard: { borderRadius: RADIUS.xl, padding: SPACING.xl, alignItems: 'center', marginBottom: SPACING.lg },
  heroNum: { color: COLORS.textPrimary, fontSize: FONT_SIZE.huge, fontWeight: '900', lineHeight: 56 },
  heroLabel: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, letterSpacing: 1.5, textTransform: 'uppercase' },
  heroSubRow: { flexDirection: 'row', alignItems: 'center', gap: SPACING.sm, marginTop: SPACING.sm },
  heroSub: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs },
  heroDot: { width: 3, height: 3, borderRadius: 2, backgroundColor: COLORS.textMuted },
  emptyCard: { alignItems: 'center', padding: SPACING.xl, backgroundColor: COLORS.surface, borderRadius: RADIUS.xl, marginBottom: SPACING.lg },
  emptyEmoji: { fontSize: 56, marginBottom: SPACING.md },
  emptyText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '700', marginBottom: SPACING.xs },
  emptySubtext: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, textAlign: 'center', marginBottom: SPACING.lg },
  startBtn: { backgroundColor: COLORS.cyan, paddingVertical: SPACING.sm, paddingHorizontal: SPACING.xl, borderRadius: RADIUS.pill },
  startBtnText: { color: COLORS.bg, fontSize: FONT_SIZE.sm, fontWeight: '700' },
  sectionLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, fontWeight: '700', letterSpacing: 1.5, textTransform: 'uppercase', marginBottom: SPACING.sm },
  towerContainer: { gap: 2, marginBottom: SPACING.md },
  towerBlock: { height: 18, borderRadius: 4, marginBottom: 2 },
  focusBlock: { shadowColor: COLORS.gold, shadowOpacity: 0.5, shadowRadius: 6, elevation: 5, height: 22 },
  calmBlock: { shadowColor: COLORS.blockCalm, shadowOpacity: 0.4, shadowRadius: 4, elevation: 3 },
  focusFloorLabel: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  focusFloorText: { color: '#0D1B2A', fontSize: 9, fontWeight: '900' },
  calmStripe: { position: 'absolute', left: '20%', right: '20%', top: '40%', height: 2, backgroundColor: 'rgba(255,255,255,0.4)', borderRadius: 1 },
  sessionDivider: { flexDirection: 'row', alignItems: 'center', gap: SPACING.sm, marginVertical: SPACING.sm },
  sessionDividerLine: { flex: 1, height: 1 },
  sessionDividerTag: { paddingHorizontal: SPACING.sm, paddingVertical: 3, borderRadius: RADIUS.pill },
  sessionDividerText: { fontSize: FONT_SIZE.xs, fontWeight: '600' },
  sessionCard: { backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, marginBottom: SPACING.sm, overflow: 'hidden' },
  sessionCardRow: { flexDirection: 'row', alignItems: 'center', padding: SPACING.md },
  sessionColorBar: { width: 4, height: 40, borderRadius: 2, marginRight: SPACING.md },
  sessionCardContent: { flex: 1 },
  sessionCardDate: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs },
  sessionCardTech: { color: COLORS.textPrimary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  sessionCardStats: { alignItems: 'flex-end' },
  sessionCardFloors: { color: COLORS.textPrimary, fontSize: FONT_SIZE.xl, fontWeight: '800' },
  sessionCardFloorsLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs },
  badgeRow: { flexDirection: 'row', flexWrap: 'wrap', gap: SPACING.xs, paddingHorizontal: SPACING.md, paddingBottom: SPACING.sm },
  badge: { backgroundColor: 'rgba(255,215,0,0.1)', paddingHorizontal: SPACING.sm, paddingVertical: 3, borderRadius: RADIUS.pill },
  badgeText: { color: COLORS.gold, fontSize: 10, fontWeight: '700' },
});
