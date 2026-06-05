import React, { useCallback, useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, ScrollView,
  Switch, TextInput, KeyboardAvoidingView, Platform,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter, useFocusEffect } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { COLORS, SPACING, FONT_SIZE, RADIUS } from '../src/constants/theme';
import { loadAppData, updateSettings, saveCustomRhythm, deleteCustomRhythm, resetAllData, CustomRhythm, AppSettings } from '../src/store/storage';

function SettingRow({ label, sub, rightEl }: { label: string; sub?: string; rightEl: React.ReactNode }) {
  return (
    <View style={s.settingRow}>
      <View style={s.settingText}>
        <Text style={s.settingLabel}>{label}</Text>
        {sub ? <Text style={s.settingSubtext}>{sub}</Text> : null}
      </View>
      {rightEl}
    </View>
  );
}

export default function Settings() {
  const router = useRouter();
  const [settings, setSettings] = useState<AppSettings>({ soundEnabled: true, vibrationEnabled: true, customRhythms: [] });
  const [showAddRhythm, setShowAddRhythm] = useState(false);
  const [rhythmName, setRhythmName] = useState('');
  const [rhythmInhale, setRhythmInhale] = useState('4');
  const [rhythmHold, setRhythmHold] = useState('0');
  const [rhythmExhale, setRhythmExhale] = useState('4');
  const [rhythmHold2, setRhythmHold2] = useState('0');
  const [showResetConfirm, setShowResetConfirm] = useState(false);

  useFocusEffect(useCallback(() => {
    loadAppData().then((data) => setSettings(data.settings));
  }, []));

  const toggleSound = async (val: boolean) => {
    const next = { ...settings, soundEnabled: val };
    setSettings(next);
    await updateSettings(next);
  };

  const toggleVibration = async (val: boolean) => {
    const next = { ...settings, vibrationEnabled: val };
    setSettings(next);
    await updateSettings(next);
  };

  const handleAddRhythm = async () => {
    const inhale = parseInt(rhythmInhale, 10) || 4;
    const hold = parseInt(rhythmHold, 10) || 0;
    const exhale = parseInt(rhythmExhale, 10) || 4;
    const hold2 = parseInt(rhythmHold2, 10) || 0;

    const rhythm: CustomRhythm = {
      id: 'custom_' + Date.now(),
      name: rhythmName.trim() || `${inhale}-${exhale} Rhythm`,
      inhale, hold, exhale, hold2,
    };

    await saveCustomRhythm(rhythm);
    const data = await loadAppData();
    setSettings(data.settings);
    setShowAddRhythm(false);
    setRhythmName('');
    setRhythmInhale('4');
    setRhythmHold('0');
    setRhythmExhale('4');
    setRhythmHold2('0');
  };

  const handleDeleteRhythm = async (id: string) => {
    await deleteCustomRhythm(id);
    const data = await loadAppData();
    setSettings(data.settings);
  };

  const handleReset = async () => {
    await resetAllData();
    router.replace('/welcome');
  };

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
              <Text style={s.headerTitle}>Settings</Text>
              <View style={{ width: 40 }} />
            </View>

            {/* Preferences */}
            <Text style={s.sectionLabel}>PREFERENCES</Text>
            <View style={s.card}>
              <SettingRow
                label="Haptic Feedback"
                sub="Vibrate on block placement"
                rightEl={
                  <Switch
                    testID="vibration-toggle"
                    value={settings.vibrationEnabled}
                    onValueChange={toggleVibration}
                    thumbColor={settings.vibrationEnabled ? COLORS.cyan : COLORS.textMuted}
                    trackColor={{ false: COLORS.surface, true: COLORS.cyan + '44' }}
                  />
                }
              />
              <View style={s.divider} />
              <SettingRow
                label="Sound Effects"
                sub="Coming soon"
                rightEl={
                  <Switch
                    testID="sound-toggle"
                    value={settings.soundEnabled}
                    onValueChange={toggleSound}
                    thumbColor={settings.soundEnabled ? COLORS.cyan : COLORS.textMuted}
                    trackColor={{ false: COLORS.surface, true: COLORS.cyan + '44' }}
                  />
                }
              />
            </View>

            {/* Custom rhythms */}
            <Text style={[s.sectionLabel, { marginTop: SPACING.lg }]}>SAVED RHYTHMS</Text>
            <View style={s.card}>
              {settings.customRhythms.length === 0 ? (
                <Text style={s.emptyText}>No saved rhythms yet</Text>
              ) : (
                settings.customRhythms.map((rhythm, idx) => (
                  <View key={rhythm.id}>
                    {idx > 0 && <View style={s.divider} />}
                    <View style={s.rhythmRow} testID={`rhythm-${rhythm.id}`}>
                      <View>
                        <Text style={s.rhythmName}>{rhythm.name}</Text>
                        <Text style={s.rhythmSub}>
                          Inhale {rhythm.inhale}s
                          {rhythm.hold > 0 ? ` · Hold ${rhythm.hold}s` : ''}
                          {' · '}Exhale {rhythm.exhale}s
                          {rhythm.hold2 > 0 ? ` · Hold ${rhythm.hold2}s` : ''}
                        </Text>
                      </View>
                      <TouchableOpacity
                        testID={`delete-rhythm-${rhythm.id}`}
                        style={s.deleteBtn}
                        onPress={() => handleDeleteRhythm(rhythm.id)}
                      >
                        <Ionicons name="trash-outline" size={18} color={COLORS.danger} />
                      </TouchableOpacity>
                    </View>
                  </View>
                ))
              )}

              {!showAddRhythm ? (
                <TouchableOpacity
                  testID="add-rhythm-button"
                  style={s.addBtn}
                  onPress={() => setShowAddRhythm(true)}
                >
                  <Ionicons name="add" size={18} color={COLORS.cyan} />
                  <Text style={s.addBtnText}>Add Custom Rhythm</Text>
                </TouchableOpacity>
              ) : (
                <View style={s.addForm}>
                  <Text style={s.addFormTitle}>New Rhythm</Text>
                  <TextInput
                    testID="rhythm-name-input"
                    style={s.nameInput}
                    value={rhythmName}
                    onChangeText={setRhythmName}
                    placeholder="Name (optional)"
                    placeholderTextColor={COLORS.textMuted}
                  />
                  <View style={s.addFormRow}>
                    {[
                      { label: 'Inhale', val: rhythmInhale, set: setRhythmInhale, id: 'add-inhale' },
                      { label: 'Hold', val: rhythmHold, set: setRhythmHold, id: 'add-hold' },
                      { label: 'Exhale', val: rhythmExhale, set: setRhythmExhale, id: 'add-exhale' },
                      { label: 'Hold 2', val: rhythmHold2, set: setRhythmHold2, id: 'add-hold2' },
                    ].map((field) => (
                      <View key={field.id} style={s.addField}>
                        <Text style={s.addFieldLabel}>{field.label}</Text>
                        <TextInput
                          testID={field.id}
                          style={s.addFieldInput}
                          value={field.val}
                          onChangeText={field.set}
                          keyboardType="number-pad"
                          maxLength={2}
                          placeholderTextColor={COLORS.textMuted}
                          placeholder="0"
                        />
                      </View>
                    ))}
                  </View>
                  <View style={s.addFormBtns}>
                    <TouchableOpacity
                      testID="cancel-add-rhythm"
                      style={s.cancelBtn}
                      onPress={() => setShowAddRhythm(false)}
                    >
                      <Text style={s.cancelBtnText}>Cancel</Text>
                    </TouchableOpacity>
                    <TouchableOpacity
                      testID="save-rhythm-button"
                      style={s.saveBtn}
                      onPress={handleAddRhythm}
                    >
                      <Text style={s.saveBtnText}>Save</Text>
                    </TouchableOpacity>
                  </View>
                </View>
              )}
            </View>

            {/* About */}
            <Text style={[s.sectionLabel, { marginTop: SPACING.lg }]}>ABOUT</Text>
            <View style={s.card}>
              <SettingRow
                label="Breath Tower Rise Builder"
                sub="Version 1.0.0 · Offline breathing tracker"
                rightEl={<Text style={s.versionBadge}>v1.0</Text>}
              />
            </View>

            {/* Danger zone */}
            <Text style={[s.sectionLabel, { marginTop: SPACING.lg, color: COLORS.danger + 'aa' }]}>DANGER ZONE</Text>
            <View style={[s.card, { borderColor: COLORS.danger + '22', borderWidth: 1 }]}>
              {!showResetConfirm ? (
                <TouchableOpacity
                  testID="reset-data-button"
                  style={s.resetBtn}
                  onPress={() => setShowResetConfirm(true)}
                >
                  <Ionicons name="refresh-circle-outline" size={20} color={COLORS.danger} />
                  <Text style={s.resetBtnText}>Reset All Data</Text>
                </TouchableOpacity>
              ) : (
                <View style={s.confirmBox}>
                  <Text style={s.confirmText}>This will erase all your sessions and tower data. Are you sure?</Text>
                  <View style={s.confirmBtns}>
                    <TouchableOpacity
                      testID="cancel-reset-button"
                      style={s.cancelBtn}
                      onPress={() => setShowResetConfirm(false)}
                    >
                      <Text style={s.cancelBtnText}>Cancel</Text>
                    </TouchableOpacity>
                    <TouchableOpacity
                      testID="confirm-reset-button"
                      style={s.dangerConfirmBtn}
                      onPress={handleReset}
                    >
                      <Text style={s.dangerConfirmText}>Yes, Reset</Text>
                    </TouchableOpacity>
                  </View>
                </View>
              )}
            </View>

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
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.surface, alignItems: 'center', justifyContent: 'center' },
  headerTitle: { color: COLORS.textPrimary, fontSize: FONT_SIZE.xl, fontWeight: '700' },
  sectionLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, fontWeight: '700', letterSpacing: 1.5, textTransform: 'uppercase', marginBottom: SPACING.sm },
  card: { backgroundColor: COLORS.surface, borderRadius: RADIUS.xl, padding: SPACING.md, marginBottom: SPACING.sm },
  settingRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingVertical: SPACING.sm },
  settingText: { flex: 1, marginRight: SPACING.md },
  settingLabel: { color: COLORS.textPrimary, fontSize: FONT_SIZE.md, fontWeight: '600' },
  settingSubtext: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, marginTop: 2 },
  divider: { height: 1, backgroundColor: 'rgba(255,255,255,0.07)', marginVertical: SPACING.xs },
  emptyText: { color: COLORS.textMuted, fontSize: FONT_SIZE.sm, padding: SPACING.sm },
  rhythmRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingVertical: SPACING.sm },
  rhythmName: { color: COLORS.textPrimary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  rhythmSub: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, marginTop: 2 },
  deleteBtn: { padding: SPACING.sm },
  addBtn: { flexDirection: 'row', alignItems: 'center', gap: SPACING.xs, paddingVertical: SPACING.sm, marginTop: SPACING.xs },
  addBtnText: { color: COLORS.cyan, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  addForm: { paddingTop: SPACING.md, gap: SPACING.sm },
  addFormTitle: { color: COLORS.textPrimary, fontSize: FONT_SIZE.sm, fontWeight: '700' },
  nameInput: { backgroundColor: COLORS.surfaceHighlight, color: COLORS.textPrimary, borderRadius: RADIUS.sm, padding: SPACING.sm, fontSize: FONT_SIZE.sm },
  addFormRow: { flexDirection: 'row', gap: SPACING.sm },
  addField: { flex: 1, alignItems: 'center' },
  addFieldLabel: { color: COLORS.textMuted, fontSize: FONT_SIZE.xs, marginBottom: 4 },
  addFieldInput: { backgroundColor: COLORS.surfaceHighlight, color: COLORS.textPrimary, borderRadius: RADIUS.sm, width: '100%', textAlign: 'center', paddingVertical: SPACING.sm, fontSize: FONT_SIZE.lg, fontWeight: '700' },
  addFormBtns: { flexDirection: 'row', gap: SPACING.sm, justifyContent: 'flex-end' },
  cancelBtn: { paddingVertical: SPACING.sm, paddingHorizontal: SPACING.md, borderRadius: RADIUS.pill, backgroundColor: COLORS.surfaceHighlight },
  cancelBtnText: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, fontWeight: '600' },
  saveBtn: { paddingVertical: SPACING.sm, paddingHorizontal: SPACING.md, borderRadius: RADIUS.pill, backgroundColor: COLORS.cyan },
  saveBtnText: { color: COLORS.bg, fontSize: FONT_SIZE.sm, fontWeight: '700' },
  versionBadge: { color: COLORS.textMuted, fontSize: FONT_SIZE.sm },
  resetBtn: { flexDirection: 'row', alignItems: 'center', gap: SPACING.sm, paddingVertical: SPACING.sm },
  resetBtnText: { color: COLORS.danger, fontSize: FONT_SIZE.md, fontWeight: '600' },
  confirmBox: { gap: SPACING.md },
  confirmText: { color: COLORS.textSecondary, fontSize: FONT_SIZE.sm, lineHeight: 22 },
  confirmBtns: { flexDirection: 'row', gap: SPACING.sm, justifyContent: 'flex-end' },
  dangerConfirmBtn: { paddingVertical: SPACING.sm, paddingHorizontal: SPACING.md, borderRadius: RADIUS.pill, backgroundColor: COLORS.danger },
  dangerConfirmText: { color: '#fff', fontSize: FONT_SIZE.sm, fontWeight: '700' },
});
