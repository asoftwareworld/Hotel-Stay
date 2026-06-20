export const DOMESTIC_CITIES = ['Oslo', 'Bergen'] as const;
export const INTERNATIONAL_CITIES = ['Paris', 'London', 'Tokyo'] as const;
export const ALL_CITIES = [...DOMESTIC_CITIES, ...INTERNATIONAL_CITIES] as const;

export type DestinationClass = 'domestic' | 'international' | 'unknown';

export function getDestinationClass(city: string): DestinationClass {
  if ((DOMESTIC_CITIES as readonly string[]).includes(city)) return 'domestic';
  if ((INTERNATIONAL_CITIES as readonly string[]).includes(city)) return 'international';
  return 'unknown';
}
