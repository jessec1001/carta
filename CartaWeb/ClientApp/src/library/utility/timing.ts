/**
 * Converts a number of milliseconds to the JavaScript default.
 * @param ms The number of milliseconds.
 * @returns A time.
 */
export function milliseconds(ms: number) {
  return ms;
}
/**
 * Converts a number of seconds to the JavaScript default.
 * @param s The number of seconds.
 * @returns A time.
 */
export function seconds(s: number) {
  return s * 1000;
}
/**
 * Converts a number of minutes to the JavaScript default.
 * @param m The number of minutes.
 * @returns A time.
 */
export function minutes(m: number) {
  return m * 60 * 1000;
}
