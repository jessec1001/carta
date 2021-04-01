/**
 * Represents a buffer to make asynchronous requests with. Only a limited
 * number of requests are dispatched simultaneously specified by a size
 * parameter. This buffer if LIFO.
 */
export default class RequestBuffer {
  size: number;
  active: number;
  queue: { function: () => Promise<any>; resolve: (value: any) => void }[];

  /**
   * Creates a new instance of the <code>RequestBuffer</code> class.
   * @param size The size of the buffer. If not specified, the default value is 16.
   */
  constructor(size?: number) {
    this.size = size ?? 64;
    this.active = 0;
    this.queue = [];
  }

  /**
   * Makes an asynchronous request that should be buffered.
   * @param promise The promise generating function that is the request.
   */
  request<T>(promise: () => Promise<T>): Promise<T> {
    return new Promise((res) => {
      // Only execute the current promise if we are not violating our size contract.
      if (this.active < this.size) {
        promise().then((value: T) => {
          // Run the next request if it exists.
          const nextPromise = this.queue.pop();
          if (nextPromise) {
            this.request(nextPromise.function).then((value: any) =>
              nextPromise.resolve(value)
            );
          }

          // The request is complete so we need to resolve.
          res(value);
          this.active--;
        });
        this.active++;
      }
      // If we cannot currently execute the promise. Add it to our queue to run later.
      else this.queue.push({ function: promise, resolve: res });
    });
  }
}
