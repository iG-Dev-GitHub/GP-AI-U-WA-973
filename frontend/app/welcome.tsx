import React, { useRef, useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, Dimensions,
  FlatList, NativeScrollEvent, NativeSyntheticEvent,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter } from 'expo-router';
import Animated, { useSharedValue, useAnimatedStyle, withTiming } from 'react-native-reanimated';
import { COLORS, SPACING, FONT_SIZE } from '../src/constants/theme';
import { markTutorialSeen } from '../src/store/storage';

const { width } = Dimensions.get('window');

const SLIDES = [
  {
    id: 'slide1',
    emoji: '🏗️',
    title: 'Choose a breathing rhythm',
    subtitle: 'raise the tower',
    body: 'Pick from 4-7-8, Box Breathing, Equal Breathing, or create your own custom rhythm.',
  },
  {
    id: 'slide2',
    emoji: '🧘',
    title: 'Breathe in sync',
    subtitle: 'stack perfect blocks',
    body: 'Follow the breathing guide and complete each cycle to add a glowing block to your tower.',
  },
  {
    id: 'slide3',
    emoji: '🌟',
    title: 'Stay focused',
    subtitle: 'reach the peak',
    body: 'Build your tower higher with every session. Earn badges, unlock Focus Floors, and track your journey.',
  },
];

// Pre-defined stars and buildings
const STARS = [
  { x: 25, y: 20 }, { x: 70, y: 45 }, { x: 130, y: 18 }, { x: 190, y: 55 },
  { x: 240, y: 28 }, { x: 300, y: 42 }, { x: 350, y: 15 }, { x: 60, y: 80 },
  { x: 160, y: 70 }, { x: 280, y: 68 }, { x: 395, y: 38 }, { x: 330, y: 72 },
];

export default function Welcome() {
  const router = useRouter();
  const [activeSlide, setActiveSlide] = useState(0);
  const listRef = useRef<FlatList>(null);

  const handleScroll = (e: NativeSyntheticEvent<NativeScrollEvent>) => {
    const idx = Math.round(e.nativeEvent.contentOffset.x / width);
    setActiveSlide(idx);
  };

  const handleNext = async () => {
    if (activeSlide < SLIDES.length - 1) {
      listRef.current?.scrollToIndex({ index: activeSlide + 1, animated: true });
    } else {
      await markTutorialSeen();
      router.replace('/home');
    }
  };

  return (
    <SafeAreaView style={s.safe} edges={['top', 'bottom']}>
      <LinearGradient colors={['#0D1B2A', '#1B263B', '#0D1B2A']} style={s.bg}>
        {/* Stars */}
        {STARS.map((star, i) => (
          <View key={i} style={[s.star, { left: star.x, top: star.y }]} />
        ))}

        {/* Slides */}
        <FlatList
          ref={listRef}
          data={SLIDES}
          horizontal
          pagingEnabled
          showsHorizontalScrollIndicator={false}
          onScroll={handleScroll}
          scrollEventThrottle={16}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <View style={[s.slide, { width }]}>
              <View style={s.emojiContainer}>
                <Text style={s.emoji}>{item.emoji}</Text>
                {/* Decorative tower blocks */}
                <View style={s.towerPreview}>
                  {[0, 1, 2, 3, 4].map((i) => (
                    <View
                      key={i}
                      style={[
                        s.previewBlock,
                        {
                          backgroundColor: i === 4 ? COLORS.blockPerfect :
                            i === 2 ? COLORS.blockCalm :
                              COLORS.techniqueBox,
                          opacity: 0.5 + i * 0.1,
                        },
                      ]}
                    />
                  ))}
                </View>
              </View>

              <View style={s.textContainer}>
                <Text style={s.title}>{item.title}</Text>
                <Text style={s.subtitle}>— {item.subtitle} —</Text>
                <Text style={s.body}>{item.body}</Text>
              </View>
            </View>
          )}
        />

        {/* Dots */}
        <View style={s.dots}>
          {SLIDES.map((_, i) => (
            <View
              key={i}
              style={[s.dot, i === activeSlide && s.dotActive]}
            />
          ))}
        </View>

        {/* Button */}
        <TouchableOpacity
          testID="welcome-next-button"
          style={s.btn}
          onPress={handleNext}
          activeOpacity={0.8}
        >
          <LinearGradient
            colors={['#00CED1', '#4A9EFF']}
            style={s.btnGrad}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
          >
            <Text style={s.btnText}>
              {activeSlide < SLIDES.length - 1 ? 'Next →' : 'Start Breathing'}
            </Text>
          </LinearGradient>
        </TouchableOpacity>
      </LinearGradient>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  bg: { flex: 1 },
  star: {
    position: 'absolute',
    width: 3,
    height: 3,
    borderRadius: 2,
    backgroundColor: 'rgba(255,255,255,0.7)',
  },
  slide: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: SPACING.xl,
    paddingBottom: 80,
  },
  emojiContainer: {
    alignItems: 'center',
    marginBottom: SPACING.xl,
  },
  emoji: {
    fontSize: 72,
    marginBottom: SPACING.md,
  },
  towerPreview: {
    alignItems: 'center',
    gap: 3,
  },
  previewBlock: {
    width: 80,
    height: 14,
    borderRadius: 4,
  },
  textContainer: {
    alignItems: 'center',
    gap: SPACING.sm,
  },
  title: {
    color: COLORS.textPrimary,
    fontSize: FONT_SIZE.xxl,
    fontWeight: '700',
    textAlign: 'center',
    letterSpacing: -0.5,
  },
  subtitle: {
    color: COLORS.cyan,
    fontSize: FONT_SIZE.md,
    fontWeight: '600',
    textAlign: 'center',
    letterSpacing: 1,
  },
  body: {
    color: COLORS.textSecondary,
    fontSize: FONT_SIZE.md,
    textAlign: 'center',
    lineHeight: 24,
    marginTop: SPACING.sm,
  },
  dots: {
    flexDirection: 'row',
    justifyContent: 'center',
    gap: SPACING.sm,
    marginBottom: SPACING.lg,
  },
  dot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: 'rgba(255,255,255,0.3)',
  },
  dotActive: {
    backgroundColor: COLORS.cyan,
    width: 24,
  },
  btn: {
    marginHorizontal: SPACING.xl,
    marginBottom: SPACING.xl,
    borderRadius: 9999,
    overflow: 'hidden',
  },
  btnGrad: {
    paddingVertical: SPACING.md,
    paddingHorizontal: SPACING.xl,
    alignItems: 'center',
  },
  btnText: {
    color: COLORS.textPrimary,
    fontSize: FONT_SIZE.lg,
    fontWeight: '700',
  },
});
