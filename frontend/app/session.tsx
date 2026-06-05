import React, { useEffect, useRef, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, Dimensions,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';
import Animated, {
  useSharedValue, useAnimatedStyle, SharedValue,
  withTiming, withSpring, withRepeat, withSequence, Easing,
} from 'react-native-reanimated';
import { useRouter, useLocalSearchParams } from 'expo-router';
import * as Haptics from 'expo-haptics';
import { Ionicons } from '@expo/vector-icons';
import { COLORS, SPACING, FONT_SIZE, RADIUS, getTechniqueColor } from '../src/constants/theme';
import { TECHNIQUES, buildCustomTechnique, determineBlockType, getPhaseColor, PhaseName, Technique } from '../src/utils/breathing';
import { BlockRecord, generateSessionId, saveSession, computeBadges } from '../src/store/storage';

const { width, height } = Dimensions.get('window');
const TOWER_AREA_H = Math.min(height * 0.42, 320);
const BLOCK_H = 20;
const BLOCK_W = 110;
const MAX_BLOCKS = 10;

// ─── City Background Data ───────────────────────────────────────────────────
const BUILDINGS = [
  { x: 0, w: 45, h: 80 }, { x: 48, w: 32, h: 105 }, { x: 83, w: 55, h: 68 },
  { x: 140, w: 38, h: 95 }, { x: 181, w: 50, h: 62 }, { x: 233, w: 35, h: 98 },
  { x: 271, w: 48, h: 76 }, { x: 321, w: 42, h: 110 }, { x: 366, w: 50, h: 72 },
];
const STARS = [
  { x: 25, y: 10 }, { x: 70, y: 28 }, { x: 130, y: 14 }, { x: 190, y: 42 },
  { x: 250, y: 20 }, { x: 310, y: 35 }, { x: 370, y: 12 }, { x: 55, y: 55 },
  { x: 175, y: 60 }, { x: 290, y: 52 }, { x: 340, y: 62 }, { x: 95, y: 45 },
];

// ─── Animated Block ──────────────────────────────────────────────────────────
function AnimatedBlock({
  block, isNewest, techniqueColor,
}: { block: BlockRecord; isNewest: boolean; techniqueColor: string }) {
  const ty = useSharedValue(isNewest ? -40 : 0);
  const sc = useSharedValue(isNewest ? 0.7 : 1);

  useEffect(() => {
    if (isNewest) {
      ty.value = withSpring(0, { damping: 14, stiffness: 100 });
      sc.value = withSpring(1, { damping: 14, stiffness: 100 });
    }
  }, []);

  const animStyle = useAnimatedStyle(() => ({
    transform: [{ translateY: ty.value }, { scaleX: sc.value }],
  }));

  const color = block.type === 'focus' || block.type === 'perfect' ? COLORS.blockPerfect
    : block.type === 'calm' ? COLORS.blockCalm
    : techniqueColor;

  return (
    <Animated.View style={[s.towerBlock, { backgroundColor: color }, animStyle]}>
      {block.type === 'focus' && <Text style={s.focusStar}>★</Text>}
      {block.type === 'calm' && <View style={s.calmLine} />}
    </Animated.View>
  );
}

// ─── Crane View ──────────────────────────────────────────────────────────────
function CraneView({ angle, techniqueColor }: { angle: SharedValue<number>; techniqueColor: string }) {
  const animStyle = useAnimatedStyle(() => ({
    transform: [{ rotate: angle.value + 'deg' }],
  }));

  return (
    <View style={s.craneWrapper}>
      <View style={s.craneMast} />
      <Animated.View style={[s.craneArm, animStyle]}>
        <View style={s.craneBeam} />
        <View style={s.craneCable} />
        <View style={[s.craneHangBlock, { backgroundColor: techniqueColor }]} />
      </Animated.View>
    </View>
  );
}

// ─── Main Session Screen ──────────────────────────────────────────────────────
export default function Session() {
  const router = useRouter();
  const params = useLocalSearchParams<{
    techniqueId: string; sessionType: string; sessionValue: string;
    customInhale?: string; customHold?: string; customExhale?: string; customHold2?: string;
  }>();

  const techniqueId = params.techniqueId ?? '4-7-8';
  const sessionType = (params.sessionType ?? 'cycles') as 'cycles' | 'time';
  const sessionValue = parseInt(params.sessionValue ?? '20', 10);
  const targetCycles = sessionType === 'cycles' ? sessionValue : Infinity;
  const targetMs = sessionType === 'time' ? sessionValue * 60 * 1000 : Infinity;

  const getTechnique = useCallback((): Technique => {
    if (techniqueId === 'custom') {
      return buildCustomTechnique(
        parseInt(params.customInhale ?? '4', 10),
        parseInt(params.customHold ?? '0', 10),
        parseInt(params.customExhale ?? '4', 10),
        parseInt(params.customHold2 ?? '0', 10),
      );
    }
    return TECHNIQUES.find((t) => t.id === techniqueId) ?? TECHNIQUES[0];
  }, [techniqueId]);

  const techniqueRef = useRef<Technique>(getTechnique());
  const techniqueColor = getTechniqueColor(techniqueId);

  // Session ref (timer state, avoids stale closures)
  const session = useRef({
    active: false,
    phaseIdx: 0,
    phaseMs: 0,
    totalMs: 0,
    cycles: 0,
    blocks: [] as BlockRecord[],
    finishing: false,
    id: generateSessionId(),
  });

  // UI State
  const [isStarted, setIsStarted] = useState(false);
  const [phase, setPhase] = useState<PhaseName>('inhale');
  const [phaseTimeLeft, setPhaseTimeLeft] = useState(techniqueRef.current.phases[0].duration);
  const [ringColor, setRingColor] = useState(getPhaseColor('inhale'));
  const [cycleCount, setCycleCount] = useState(0);
  const [blocks, setBlocks] = useState<BlockRecord[]>([]);
  const [newestBlockId, setNewestBlockId] = useState<string | null>(null);
  const [shouldFinish, setShouldFinish] = useState(false);

  // Animations
  const breathScale = useSharedValue(0.65);
  const craneAngle = useSharedValue(0);
  const flashOp = useSharedValue(0);
  const completionGlow = useSharedValue(0);

  // Styles
  const breathStyle = useAnimatedStyle(() => ({ transform: [{ scale: breathScale.value }] }));
  const breathGlowStyle = useAnimatedStyle(() => ({
    transform: [{ scale: breathScale.value * 1.3 }],
    opacity: 0.25,
  }));
  const flashStyle = useAnimatedStyle(() => ({ opacity: flashOp.value }));
  const completionStyle = useAnimatedStyle(() => ({ opacity: completionGlow.value }));

  // ── Timer ──────────────────────────────────────────────────────────────────
  useEffect(() => {
    const interval = setInterval(() => {
      if (!session.current.active) return;

      const TICK = 100;
      session.current.phaseMs += TICK;
      session.current.totalMs += TICK;

      const phases = techniqueRef.current.phases;
      const ph = phases[session.current.phaseIdx];
      const phDurMs = ph.duration * 1000;

      setPhaseTimeLeft(Math.max(0, (phDurMs - session.current.phaseMs) / 1000));

      if (session.current.phaseMs >= phDurMs) {
        const nextIdx = (session.current.phaseIdx + 1) % phases.length;
        session.current.phaseIdx = nextIdx;
        session.current.phaseMs = 0;

        const nextPh = phases[nextIdx];
        setPhase(nextPh.name);
        setRingColor(getPhaseColor(nextPh.name));
        setPhaseTimeLeft(nextPh.duration);

        // Animate ring
        if (nextPh.name === 'inhale') {
          breathScale.value = withTiming(1.15, { duration: nextPh.duration * 1000, easing: Easing.out(Easing.quad) });
        } else if (nextPh.name === 'hold' || nextPh.name === 'hold2') {
          breathScale.value = withRepeat(
            withSequence(withTiming(1.2, { duration: 600 }), withTiming(1.1, { duration: 600 })),
            -1, true,
          );
        } else {
          breathScale.value = withTiming(0.65, { duration: nextPh.duration * 1000, easing: Easing.in(Easing.quad) });
        }

        // Cycle complete?
        if (nextIdx === 0) {
          session.current.cycles++;
          const n = session.current.cycles;
          setCycleCount(n);

          const blockType = determineBlockType(n);
          const block: BlockRecord = { id: 'b_' + n, type: blockType, cycleNum: n, techniqueId };
          session.current.blocks.push(block);
          setBlocks([...session.current.blocks]);
          setNewestBlockId(block.id);

          flashOp.value = withSequence(
            withTiming(0.7, { duration: 80 }),
            withTiming(0, { duration: 450 }),
          );
          Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light).catch(() => {});

          if (blockType === 'focus') {
            Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success).catch(() => {});
          }

          if (sessionType === 'cycles' && n >= targetCycles && !session.current.finishing) {
            session.current.finishing = true;
            setTimeout(() => setShouldFinish(true), 1000);
          }
        }
      }

      if (sessionType === 'time' && session.current.totalMs >= targetMs && !session.current.finishing) {
        session.current.finishing = true;
        setTimeout(() => setShouldFinish(true), 1000);
      }
    }, 100);

    return () => clearInterval(interval);
  }, []);

  // ── Handle session end ─────────────────────────────────────────────────────
  useEffect(() => {
    if (!shouldFinish) return;
    session.current.active = false;

    completionGlow.value = withRepeat(
      withSequence(withTiming(1, { duration: 400 }), withTiming(0.4, { duration: 400 })),
      3, false,
    );

    const durationSec = Math.floor(session.current.totalMs / 1000);
    const bs = session.current.blocks;
    const perfect = bs.filter((b) => b.type === 'perfect' || b.type === 'focus').length;
    const calm = bs.filter((b) => b.type === 'calm').length;
    const focus = bs.filter((b) => b.type === 'focus').length;

    const record = {
      id: session.current.id,
      date: new Date().toISOString(),
      techniqueId,
      techniqueName: techniqueRef.current.name,
      cyclesCompleted: session.current.cycles,
      perfectBlocks: perfect,
      calmBlocks: calm,
      focusFloors: focus,
      totalFloors: session.current.cycles,
      badges: [] as string[],
      durationSeconds: durationSec,
      blocks: bs,
    };
    record.badges = computeBadges(record);

    saveSession(record).catch(console.error);
    setTimeout(() => {
      router.replace({ pathname: '/summary', params: { sessionId: record.id } });
    }, 1200);
  }, [shouldFinish]);

  // ── Start Session ──────────────────────────────────────────────────────────
  const startSession = useCallback(() => {
    session.current.active = true;
    session.current.phaseIdx = 0;
    session.current.phaseMs = 0;
    session.current.totalMs = 0;
    session.current.cycles = 0;
    session.current.blocks = [];
    session.current.finishing = false;

    setIsStarted(true);
    setPhase('inhale');
    setRingColor(getPhaseColor('inhale'));
    setPhaseTimeLeft(techniqueRef.current.phases[0].duration);

    breathScale.value = 0.65;
    breathScale.value = withTiming(1.15, {
      duration: techniqueRef.current.phases[0].duration * 1000,
      easing: Easing.out(Easing.quad),
    });

    craneAngle.value = withRepeat(
      withSequence(
        withTiming(12, { duration: 2500, easing: Easing.inOut(Easing.sin) }),
        withTiming(-12, { duration: 2500, easing: Easing.inOut(Easing.sin) }),
      ),
      -1, false,
    );
  }, []);

  // ── End Session (early) ────────────────────────────────────────────────────
  const handleEndSession = useCallback(() => {
    if (!session.current.active || session.current.finishing) return;
    session.current.finishing = true;
    setShouldFinish(true);
  }, []);

  const visibleBlocks = blocks.slice(-MAX_BLOCKS);
  const phaseName = phase === 'inhale' ? 'Inhale'
    : phase === 'hold' || phase === 'hold2' ? 'Hold'
    : 'Exhale';

  return (
    <SafeAreaView style={s.safe} edges={['top', 'bottom']}>
      {/* Flash overlay */}
      <Animated.View style={[s.flashOverlay, flashStyle]} pointerEvents="none" />
      <Animated.View style={[s.completionOverlay, completionStyle]} pointerEvents="none" />

      <LinearGradient colors={['#0D1B2A', '#1B263B']} style={s.bg}>
        {/* ── Header ── */}
        <View style={s.header}>
          <TouchableOpacity testID="end-session-early-button" onPress={handleEndSession} style={s.exitBtn}>
            <Ionicons name="close" size={20} color={COLORS.textSecondary} />
          </TouchableOpacity>
          <View style={s.headerCenter}>
            <Text style={s.techniqueLabel}>{techniqueRef.current.name}</Text>
          </View>
          <View testID="cycle-counter" style={s.cycleTag}>
            <Text style={s.cycleText}>
              {cycleCount}{sessionType === 'cycles' && targetCycles !== Infinity ? '/' + targetCycles : ''}
            </Text>
            <Text style={s.cycleLabel}> cycles</Text>
          </View>
        </View>

        {/* ── Tower Area ── */}
        <View style={[s.towerArea, { height: TOWER_AREA_H }]}>
          {/* Stars */}
          {STARS.map((st, i) => (
            <View key={i} style={[s.star, { left: st.x, top: st.y }]} />
          ))}

          {/* Crane */}
          {isStarted && <CraneView angle={craneAngle} techniqueColor={techniqueColor} />}

          {/* Tower blocks - grows from bottom */}
          <View style={s.towerStack}>
            {visibleBlocks.map((block) => (
              <AnimatedBlock
                key={block.id}
                block={block}
                isNewest={block.id === newestBlockId}
                techniqueColor={techniqueColor}
              />
            ))}
          </View>

          {/* City silhouette */}
          <View style={s.cityRow}>
            {BUILDINGS.map((b, i) => (
              <View key={i} style={[s.building, { left: b.x, width: b.w, height: b.h }]} />
            ))}
          </View>
        </View>

        {/* ── Breathing Area ── */}
        <View style={s.breathArea}>
          {/* Phase label */}
          <Text style={[s.phaseLabel, { color: ringColor }]}>{isStarted ? phaseName : 'Ready'}</Text>

          {/* Breathing ring */}
          <View style={s.ringContainer}>
            <Animated.View style={[s.ringGlow, { borderColor: ringColor }, breathGlowStyle]} />
            <Animated.View style={[s.ring, { borderColor: ringColor }, breathStyle]}>
              <View style={s.ringCenter}>
                {isStarted ? (
                  <>
                    <Text style={[s.timerNum, { color: ringColor }]}>
                      {phaseTimeLeft.toFixed(1)}
                    </Text>
                    <Text style={s.timerUnit}>sec</Text>
                  </>
                ) : (
                  <Text style={s.readyText}>Tap Begin</Text>
                )}
              </View>
            </Animated.View>
          </View>

          {/* Controls */}
          {!isStarted ? (
            <TouchableOpacity
              testID="begin-session-button"
              style={s.beginBtn}
              onPress={startSession}
              activeOpacity={0.85}
            >
              <LinearGradient
                colors={['#00CED1', '#4A9EFF']}
                style={s.beginBtnGrad}
                start={{ x: 0, y: 0 }}
                end={{ x: 1, y: 0 }}
              >
                <Ionicons name="play" size={20} color="#fff" />
                <Text style={s.beginBtnText}>Begin</Text>
              </LinearGradient>
            </TouchableOpacity>
          ) : (
            <TouchableOpacity
              testID="end-session-button"
              style={s.endBtn}
              onPress={handleEndSession}
              activeOpacity={0.8}
            >
              <Text style={s.endBtnText}>End Session</Text>
            </TouchableOpacity>
          )}
        </View>
      </LinearGradient>
    </SafeAreaView>
  );
}

const RING_SIZE = 170;

const s = StyleSheet.create({
  safe: { flex: 1 },
  bg: { flex: 1 },

  flashOverlay: {
    position: 'absolute', top: 0, left: 0, right: 0, bottom: 0,
    backgroundColor: 'rgba(255,255,255,0.12)', zIndex: 100,
  },
  completionOverlay: {
    position: 'absolute', top: 0, left: 0, right: 0, bottom: 0,
    backgroundColor: 'rgba(0,206,209,0.08)', zIndex: 100,
  },

  header: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingHorizontal: SPACING.md, paddingVertical: SPACING.sm,
  },
  exitBtn: {
    width: 36, height: 36, borderRadius: 18,
    backgroundColor: COLORS.surface, alignItems: 'center', justifyContent: 'center',
  },
  headerCenter: { flex: 1, alignItems: 'center' },
  techniqueLabel: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  cycleTag: { flexDirection: 'row', alignItems: 'baseline' },
  cycleText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '800' },
  cycleLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs },

  towerArea: { width: '100%', overflow: 'hidden', position: 'relative' },

  star: { position: 'absolute', width: 2, height: 2, borderRadius: 1, backgroundColor: 'rgba(255,255,255,0.6)' },

  craneWrapper: {
    position: 'absolute', top: 8, left: width / 2 - 65, width: 130, height: 80,
  },
  craneMast: {
    position: 'absolute', right: 10, top: 20, width: 8, height: 60,
    backgroundColor: COLORS.crane, borderRadius: 4,
  },
  craneArm: { position: 'absolute', top: 20, right: 10, width: 110, height: 60 },
  craneBeam: { position: 'absolute', top: 0, left: 0, right: 0, height: 6, backgroundColor: COLORS.crane, borderRadius: 3 },
  craneCable: { position: 'absolute', top: 6, left: 12, width: 2, height: 30, backgroundColor: COLORS.crane },
  craneHangBlock: { position: 'absolute', top: 36, left: 3, width: 20, height: 14, borderRadius: 3 },

  towerStack: {
    position: 'absolute', bottom: 62, left: 0, right: 0,
    alignItems: 'center', gap: 2,
    flexDirection: 'column-reverse',
  },
  towerBlock: {
    width: BLOCK_W, height: BLOCK_H, borderRadius: 5,
    alignItems: 'center', justifyContent: 'center',
    shadowColor: '#FFD700', shadowOpacity: 0.3, shadowRadius: 4, elevation: 3,
  },
  focusStar: { color: '#0D1B2A', fontSize: 10, fontWeight: '900' },
  calmLine: { width: '60%', height: 2, backgroundColor: 'rgba(255,255,255,0.4)', borderRadius: 1 },

  cityRow: { position: 'absolute', bottom: 0, left: 0, right: 0, height: 60 },
  building: { position: 'absolute', bottom: 0, backgroundColor: '#0A1520' },

  breathArea: { flex: 1, alignItems: 'center', justifyContent: 'space-evenly', paddingHorizontal: SPACING.md },

  phaseLabel: { fontSize: FONT_SIZE.xl, fontWeight: '700', letterSpacing: 1 },

  ringContainer: { alignItems: 'center', justifyContent: 'center' },
  ringGlow: {
    position: 'absolute', width: RING_SIZE, height: RING_SIZE,
    borderRadius: RING_SIZE / 2, borderWidth: 20,
    backgroundColor: 'transparent',
  },
  ring: {
    width: RING_SIZE, height: RING_SIZE, borderRadius: RING_SIZE / 2,
    borderWidth: 4, backgroundColor: 'rgba(255,255,255,0.04)',
    alignItems: 'center', justifyContent: 'center',
  },
  ringCenter: { alignItems: 'center' },
  timerNum: { fontSize: FONT_SIZE.huge, fontWeight: '800', lineHeight: 54 },
  timerUnit: { color: COLORS.textMuted, fontSize: FONT_SIZE.sm },
  readyText: { color: COLORS.textMuted, fontSize: FONT_SIZE.md, fontWeight: '600' },

  beginBtn: { borderRadius: RADIUS.pill, overflow: 'hidden', width: '80%' },
  beginBtnGrad: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: SPACING.sm, paddingVertical: SPACING.md },
  beginBtnText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '700' },
  endBtn: {
    paddingVertical: SPACING.sm, paddingHorizontal: SPACING.xl,
    borderRadius: RADIUS.pill, borderWidth: 1, borderColor: 'rgba(255,255,255,0.2)',
  },
  endBtnText: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
});
