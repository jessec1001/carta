import { useEffect, useRef } from "react";

const useUpdateAlert = (name: string, value: any) => {
  const ref = useRef(value);
  useEffect(() => {
    console.log(`ALERT: Setup for ${name} as ${value}!`);
  }, []);
  useEffect(() => {
    if (value !== ref.current)
      console.log(`ALERT: Updated ${name} to ${value}!`);
    ref.current = value;
  });
};

export default useUpdateAlert;
