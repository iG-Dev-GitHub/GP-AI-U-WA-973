import React, { useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, ScrollView,
  TextInput, KeyboardAvoidingView, Platform,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { COLORS, SPACING, FONT_SIZE, RADIUS, getTechniqueColor } from '../src/constants/theme';
import { TECHNIQUES } from '../src/utils/breathing';

const SESSION_LENGTHS_CYCLES = [
  { label: '10 cycles', value: 10 },
  { label: '15 cycles', value: 15 },
  { label: '20 cycles', value: 20 },
  { label: '30 cycles', value: 30 },
];

const SESSION_LENGTHS_TIME = [
  { label: '5 min', value: 5 },
  { label: '10 min', value: 10 },
  { label: '15 min', value: 15 },
  { label: '20 min', value: 20 },
];

export default function TechniqueScreen() {
  const router = useRouter();
  const [selectedId, setSelectedId] = useState('4-7-8');
  const [sessionMode, setSessionMode] = useState<'cycles' | 'time'>('cycles');
  const [sessionValue, setSessionValue] = useState(20);
  const [showCustom, setShowCustom] = useState(false);
  const [customInhale, setCustomInhale] = useState('4');
  const [customHold, setCustomHold] = useState('2');
  const [customExhale, setCustomExhale] = useState('4');
  const [customHold2, setCustomHold2] = useState('0');

  const handleStart = () => {
    const params: Record<string, string> = {
      techniqueId: selectedId,
      sessionType: sessionMode,
      sessionValue: String(sessionValue),
    };
    if (selectedId === 'custom') {
      params.customInhale = customInhale || '4';
      params.customHold = customHold || '0';
      params.customExhale = customExhale || '4';
      params.customHold2 = customHold2 || '0';
    }
    router.push({ pathname: '/session', params });
  };

  const sessionLengths = sessionMode === 'cycles' ? SESSION_LENGTHS_CYCLES : SESSION_LENGTHS_TIME;

  return (
    <SafeAreaView style={s.safe} edges={['top', 'bottom']}>
      <LinearGradient colors={['#0D1B2A', '#1B263B']} style={s.bg}>
        <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : 'height'}>
          <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={s.scroll}>
            {/* Header */}
            <View style={s.header}>
              <TouchableOpacity testID="back-button" style={s.backBtn} onPress={() => router.back()}>
                <Ionicons name="arrow-back" size={22} color={COLORS.textPrimary} />
              </TouchableOpacity>
              <Text style={s.headerTitle}>Choose Technique</Text>
              <View style={{ width: 40 }} />
            </View>

            {/* Techniques */}
            <Text style={s.sectionLabel}>BREATHING TECHNIQUE</Text>
            {TECHNIQUES.map((tech) => {
              const isSelected = selectedId === tech.id;
              const techColor = getTechniqueColor(tech.id);
              return (
                <TouchableOpacity
                  key={tech.id}
                  testID={`technique-${tech.id}`}
                  style={[s.techCard, isSelected && { borderColor: techColor, borderWidth: 2 }]}
                  onPress={() => { setSelectedId(tech.id); if (tech.id === 'custom') setShowCustom(true); }}
                  activeOpacity={0.8}
                >
                  <View style={[s.techColorBar, { backgroundColor: techColor }]} />
                  <View style={s.techContent}>
                    <View style={s.techHeader}>
                      <Text style={s.techName}>{tech.name}</Text>
                      {isSelected && <Ionicons name="checkmark-circle" size={20} color={techColor} />}
                    </View>
                    <Text style={[s.techRhythm, { color: techColor }]}>{tech.rhythm}</Text>
                    <Text style={s.techDesc} numberOfLines={2}>{tech.description}</Text>
                  </View>
                </TouchableOpacity>
              );
            })}

            {/* Custom rhythm inputs */}
            {selectedId === 'custom' && (
              <View style={s.customCard}>
                <Text style={s.customTitle}>Custom Rhythm (seconds)</Text>
                <View style={s.customRow}>
                  <View style={s.customField}>
                    <Text style={s.customLabel}>Inhale</Text>
                    <TextInput
                      testID="custom-inhale-input"
                      style={s.customInput}
                      value={customInhale}
                      onChangeText={setCustomInhale}
                      keyboardType="number-pad"
                      maxLength={2}
                      placeholderTextColor={COLORS.textMuted}
                      placeholder="4"
                    />
                  </View>
                  <View style={s.customField}>
                    <Text style={s.customLabel}>Hold</Text>
                    <TextInput
                      testID="custom-hold-input"
                      style={s.customInput}
                      value={customHold}
                      onChangeText={setCustomHold}
                      keyboardType="number-pad"
                      maxLength={2}
                      placeholderTextColor={COLORS.textMuted}
                      placeholder="0"
                    />
                  </View>
                  <View style={s.customField}>
                    <Text style={s.customLabel}>Exhale</Text>
                    <TextInput
                      testID="custom-exhale-input"
                      style={s.customInput}
                      value={customExhale}
                      onChangeText={setCustomExhale}
                      keyboardType="number-pad"
                      maxLength={2}
                      placeholderTextColor={COLORS.textMuted}
                      placeholder="4"
                    />
                  </View>
                  <View style={s.customField}>
                    <Text style={s.customLabel}>Hold 2</Text>
                    <TextInput
                      testID="custom-hold2-input"
                      style={s.customInput}
                      value={customHold2}
                      onChangeText={setCustomHold2}
                      keyboardType="number-pad"
                      maxLength={2}
                      placeholderTextColor={COLORS.textMuted}
                      placeholder="0"
                    />
                  </View>
                </View>
              </View>
            )}

            {/* Session mode */}
            <Text style={[s.sectionLabel, { marginTop: SPACING.lg }]}>SESSION LENGTH</Text>
            <View style={s.modeRow}>
              <TouchableOpacity
                testID="mode-cycles"
                style={[s.modeBtn, sessionMode === 'cycles' && s.modeBtnActive]}
                onPress={() => { setSessionMode('cycles'); setSessionValue(20); }}
              >
                <Text style={[s.modeBtnText, sessionMode === 'cycles' && s.modeBtnTextActive]}>Cycles</Text>
              </TouchableOpacity>
              <TouchableOpacity
                testID="mode-time"
                style={[s.modeBtn, sessionMode === 'time' && s.modeBtnActive]}
                onPress={() => { setSessionMode('time'); setSessionValue(10); }}
              >
                <Text style={[s.modeBtnText, sessionMode === 'time' && s.modeBtnTextActive]}>Time</Text>
              </TouchableOpacity>
            </View>

            <View style={s.lengthRow}>
              {sessionLengths.map((item) => (
                <TouchableOpacity
                  key={item.value}
                  testID={`length-${item.value}`}
                  style={[s.lengthBtn, sessionValue === item.value && s.lengthBtnActive]}
                  onPress={() => setSessionValue(item.value)}
                >
                  <Text style={[s.lengthBtnText, sessionValue === item.value && s.lengthBtnTextActive]}>
                    {item.label}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>

            {/* Start button */}
            <TouchableOpacity
              testID="start-session-button"
              style={s.startBtn}
              onPress={handleStart}
              activeOpacity={0.85}
            >
              <LinearGradient
                colors={['#00CED1', '#4A9EFF']}
                style={s.startBtnGrad}
                start={{ x: 0, y: 0 }}
                end={{ x: 1, y: 0 }}
              >
                <Ionicons name="play" size={20} color="#fff" />
                <Text style={s.startBtnText}>Start Session</Text>
              </LinearGradient>
            </TouchableOpacity>
            <View style={{ height: SPACING.xl }} />
          </ScrollView>
        </KeyboardAvoidingView>
      </LinearGradient>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  bg: { flex: 1 },
  scroll: { paddingHorizontal: SPACING.md, paddingTop: SPACING.sm },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: SPACING.lg },
  backBtn: { width: 40, height: 40, alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.surface, borderRadius: RADIUS.pill },
  headerTitle: { color: COLORS.textPrimary, fontSize: FONT_SIZE.xl, fontWeight: '700' },
  sectionLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, fontWeight: '700', letterSpacing: 1.5, textTransform: 'uppercase', marginBottom: SPACING.sm },
  techCard: { backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, flexDirection: 'row', marginBottom: SPACING.sm, borderWidth: 2, borderColor: 'transparent', overflow: 'hidden' },
  techColorBar: { width: 6 },
  techContent: { flex: 1, padding: SPACING.md },
  techHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 },
  techName: { color: COLORS.textPrimary, fontSize: FONT_SIZE.md, fontWeight: '700' },
  techRhythm: { fontSize: FONT_SIZE.lg, fontWeight: '800', letterSpacing: 2, marginBottom: 6 },
  techDesc: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, lineHeight: 20 },
  customCard: { backgroundColor: COLORS.surface, borderRadius: RADIUS.lg, padding: SPACING.md, marginBottom: SPACING.sm, borderWidth: 1, borderColor: COLORS.techniqueCustom + '44' },
  customTitle: { color: COLORS.textPrimary, fontSize: FONT_SIZE.sm, fontWeight: '600', marginBottom: SPACING.md },
  customRow: { flexDirection: 'row', gap: SPACING.sm },
  customField: { flex: 1, alignItems: 'center' },
  customLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, marginBottom: SPACING.xs },
  customInput: { backgroundColor: COLORS.surfaceHighlight, color: COLORS.textPrimary, borderRadius: RADIUS.sm, width: '100%', textAlign: 'center', paddingVertical: SPACING.sm, fontSize: FONT_SIZE.lg, fontWeight: '700' },
  modeRow: { flexDirection: 'row', gap: SPACING.sm, marginBottom: SPACING.sm },
  modeBtn: { flex: 1, paddingVertical: SPACING.sm, alignItems: 'center', backgroundColor: COLORS.surface, borderRadius: RADIUS.md },
  modeBtnActive: { backgroundColor: COLORS.cyan },
  modeBtnText: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  modeBtnTextActive: { color: COLORS.bg, fontWeight: '700' },
  lengthRow: { flexDirection: 'row', flexWrap: 'wrap', gap: SPACING.sm, marginBottom: SPACING.lg },
  lengthBtn: { paddingVertical: SPACING.sm, paddingHorizontal: SPACING.md, backgroundColor: COLORS.surface, borderRadius: RADIUS.pill, borderWidth: 1, borderColor: 'transparent' },
  lengthBtnActive: { borderColor: COLORS.cyan },
  lengthBtnText: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  lengthBtnTextActive: { color: COLORS.cyan, fontWeight: '700' },
  startBtn: { borderRadius: RADIUS.pill, overflow: 'hidden', marginTop: SPACING.sm },
  startBtnGrad: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: SPACING.sm, paddingVertical: SPACING.md },
  startBtnText: { color: COLORS.textPrimary, fontSize: FONT_SIZE.lg, fontWeight: '700' },
});
