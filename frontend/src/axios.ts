import axios from 'axios'

// Set base URL for all requests
axios.defaults.baseURL = '/api'

// Set default headers
axios.defaults.headers.common['Content-Type'] = 'application/json'

// Add request interceptor (optional)
axios.interceptors.request.use(
  (config) => {
    // You can add auth tokens here if needed
    // const token = localStorage.getItem('token')
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`
    // }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Add response interceptor for global error handling
axios.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    // Global error handling
    const message = error.response?.data?.message || error.message || 'An error occurred'
    console.error('API Error:', message)
    
    // You can show a global notification here
    // or handle specific status codes globally
    if (error.response?.status === 401) {
      // Handle unauthorized
      localStorage.removeItem('userEmail')
      window.location.href = '/'
    }
    
    return Promise.reject(error)
  }
)

export default axios
