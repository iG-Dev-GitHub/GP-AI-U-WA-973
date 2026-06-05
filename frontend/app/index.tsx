import { useEffect } from 'react';
import { View, ActivityIndicator, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';
import { loadAppData } from '../src/store/storage';
import { COLORS } from '../src/constants/theme';

export default function Index() {
  const router = useRouter();

  useEffect(() => {
    loadAppData().then((data) => {
      if (data.hasSeenTutorial) {
        router.replace('/home');
      } else {
        router.replace('/welcome');
      }
    });
  }, []);

  return (
    <View style={s.container}>
      <ActivityIndicator color={COLORS.cyan} size="large" />
    </View>
  );
}

const s = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.bg,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
